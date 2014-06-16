using System;
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

        private CstGenerator Generator;
        private JavaCodeManipulator Manipulator;

        public Program() {
            Generator = CstGenerators.JavaUsingAntlr3;
            Manipulator = new JavaCodeManipulator();
        }

        public void Instrument(string path) {
            var backupPath = path + BackupExtension;
            if (!File.Exists(backupPath)) {
                File.Copy(path, backupPath);
            }

            var tree = Generator.GenerateTreeFromCodePath(backupPath);
            Manipulator.SupplementBlock(tree);
            Manipulator.ModifyStatements(tree);
            Manipulator.ModifyBranches(tree);
            // Check whether generated code is parasable or not
            Generator.GenerateTreeFromCodeText(tree.Code, true);

            File.WriteAllText(path, tree.Code);
            var metadataPath = path + MetadataExtension;
            File.WriteAllText(metadataPath, CstNodeToXml(tree).ToString());
        }

        private XElement CstNodeToXml(CstNode node) {
            var element = new XElement(node.Name);
            element.SetAttributeValue(Code2XmlConstants.IdAttributeName, node.RuleId);
            if (Manipulator.StatementToId.ContainsKey(node)) {
                element.SetAttributeValue(StatementIdAttributeName, Manipulator.StatementToId[node]);
            }
            if (Manipulator.BranchToId.ContainsKey(node)) {
                element.SetAttributeValue(BranchIdAttributeName, Manipulator.BranchToId[node]);
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
            var paths = args.SelectMany(arg => {
                if (Directory.Exists(arg)) {
                    return Directory.GetFiles(arg, "*.java", SearchOption.AllDirectories);
                } else if (File.Exists(arg)) {
                    return Enumerable.Repeat(arg, 1);
                } else {
                    return Enumerable.Repeat(arg, 0);
                }
            }).Select(Path.GetFullPath).Distinct();

            var program = new Program();
            foreach (var path in paths) {
                program.Instrument(path);
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Statement: " + program.Manipulator.StatementCount);
            Console.WriteLine("Branch: " + program.Manipulator.BranchCount);
        }
    }
}