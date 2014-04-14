using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code2Xml.Core.Generators;

namespace SimpleOccf {
    internal class Program {
        private const string BackupExtension = ".soccf_backup";

        private static void Main(string[] args) {
            var iStatement = 0;
            var paths = args.SelectMany(arg => {
                if (Directory.Exists(arg)) {
                    return Directory.GetFiles(arg, "*.java", SearchOption.AllDirectories);
                } else if (File.Exists(arg)) {
                    return Enumerable.Repeat(arg, 1);
                } else {
                    return Enumerable.Repeat(arg, 0);
                }
            }).Select(Path.GetFullPath).Distinct();

            foreach (var path in paths) {
                var backupPath = path + BackupExtension;
                if (!File.Exists(backupPath)) {
                    File.Copy(path, backupPath);
                }
                var gen = CstGenerators.JavaUsingAntlr3;
                var tree = gen.GenerateTreeFromCodePath(backupPath);

                foreach (var node in FindLackingBlockNodes(tree)) {
                    node.InsertCodeBeforeSelf("{");
                    node.InsertCodeAfterSelf("}");
                }

                foreach (var stmt in JavaElements.Statement(tree)) {
                    stmt.InsertCodeAfterSelf("randoop.multi.OCCF.stmt(" + iStatement + ");");
                    iStatement++;
                }

                gen.GenerateTreeFromCodeText(tree.Code, true);

                File.WriteAllText(path, tree.Code);
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine(iStatement);
        }

        private static IEnumerable<CstNode> FindLackingBlockNodes(CstNode root) {
            var ifs = JavaElements.If(root)
                    .SelectMany(JavaElements.IfAndElseProcesses);
            var whiles = JavaElements.While(root)
                    .Select(JavaElements.WhileProcess);
            var dos = JavaElements.DoWhile(root)
                    .Select(JavaElements.DoWhileProcess);
            var fors = JavaElements.For(root)
                    .Select(JavaElements.ForProcess);

            return ifs.Concat(whiles)
                    .Concat(dos)
                    .Concat(fors);
        }
    }
}