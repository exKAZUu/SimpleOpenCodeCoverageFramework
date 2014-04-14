using System;
using System.IO;
using System.Linq;
using Code2Xml.Core.Generators;

namespace SimpleOccf {
    internal class Program {
        private const string BackupExtension = ".soccf_backup";

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

            var gen = CstGenerators.JavaUsingAntlr3;
            var man = new JavaCodeManipulator();

            foreach (var path in paths) {
                var backupPath = path + BackupExtension;
                if (!File.Exists(backupPath)) {
                    File.Copy(path, backupPath);
                }
                var tree = gen.GenerateTreeFromCodePath(backupPath);

                man.SupplementBlock(tree);
                man.ModifyStatements(tree);
                man.ModifyBranches(tree);

                // Check whether generated code is parasable or not
                gen.GenerateTreeFromCodeText(tree.Code, true);

                File.WriteAllText(path, tree.Code);
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Statement: " + man.StatementCount);
            Console.WriteLine("Branch: " + man.BranchCount);
        }
    }
}