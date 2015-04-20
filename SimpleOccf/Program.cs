using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Code2Xml.Core.Generators;
using Code2Xml.Core;

namespace SimpleOccf {
    internal class Program {
        private const string BackupExtension = ".soccf_backup";
        private const string MetadataExtension = ".soccf.xml";
        private const string StatementIdAttributeName = "statement_id";
        private const string BranchIdAttributeName = "branch_id";

        private readonly CstGenerator _generator;
        private readonly JavaCodeManipulator _manipulator;

        public Program() {
            _generator = CstGenerators.JavaUsingAntlr3;
            _manipulator = new JavaCodeManipulator();
        }

        public void Instrument(string path) {
            var backupPath = path + BackupExtension;
            if (!File.Exists(backupPath)) {
                File.Copy(path, backupPath);
            }

            var tree = _generator.GenerateTreeFromCodePath(backupPath);
            _manipulator.SupplementBlock(tree);
            _manipulator.ModifyStatements(tree);
            _manipulator.ModifyBranches(tree);
            // Check whether generated code is parasable or not
            _generator.GenerateTreeFromCodeText(tree.Code, true);

            File.WriteAllText(path, tree.Code);
            var metadataPath = path + MetadataExtension;
            File.WriteAllText(metadataPath, CstNodeToXml(tree).ToString());
        }

        private XElement CstNodeToXml(CstNode node) {
            var element = new XElement(node.Name);
            element.SetAttributeValue(Code2XmlConstants.IdAttributeName, node.RuleId);
            if (_manipulator.StatementToId.ContainsKey(node)) {
                element.SetAttributeValue(StatementIdAttributeName, _manipulator.StatementToId[node]);
            }
            if (_manipulator.BranchToId.ContainsKey(node)) {
                element.SetAttributeValue(BranchIdAttributeName, _manipulator.BranchToId[node]);
            }
            if (node.Token == null) {
                foreach (var child in node.Children()) {
                    element.Add(CstNodeToXml(child));
                }
            } else {
                foreach (var token in node.Hiddens) {
                    element.Add(token.ToXmlFromHiddenToken());
                }
                element.Add(node.Token.ToXml());
            }
            return element;
        }

        private static void Main(string[] args) {
            var paths = ExtractPaths(args);
            if (paths.Count == 0) {
                Console.WriteLine("SimpleOccf.exe arg1 arg2 ...");
                Console.WriteLine("  args: Java source code files or directories");
                return;
            }

            var program = new Program();
            foreach (var path in paths) {
                program.Instrument(path);
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Statement: " + program._manipulator.StatementCount);
            Console.WriteLine("Branch: " + program._manipulator.BranchCount);
        }

        private static IList<string> ExtractPaths(IEnumerable<string> paths) {
            return paths.SelectMany(arg => {
                if (Directory.Exists(arg)) {
                    return Directory.GetFiles(arg, "*.java", SearchOption.AllDirectories);
                }
                var repeatCount = File.Exists(arg) ? 1 : 0;
                return Enumerable.Repeat(arg, repeatCount);
            }).Select(Path.GetFullPath).Distinct().ToList();
        }
    }
}