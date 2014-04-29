﻿using System.Collections.Generic;
using System.Linq;
using Code2Xml.Core.Generators;

namespace SimpleOccf {
    public class JavaCodeManipulator {
        public int StatementCount { get; private set; }
        public int BranchCount { get; private set; }

        public JavaCodeManipulator() {}

        public void SupplementBlock(CstNode root) {
            foreach (var node in FindLackingBlockNodes(root)) {
                node.InsertCodeBeforeSelf("{");
                node.InsertCodeAfterSelf("}");
            }
        }

        public void ModifyStatements(CstNode root) {
            foreach (var stmt in FindStatementNodes(root)) {
                stmt.InsertCodeBeforeSelf("soccf.Gateway.stmt(" + StatementCount + ");");
                StatementCount++;
            }
        }

        public void ModifyBranches(CstNode root) {
            foreach (var branch in FindBranchNodes(root)) {
                if (branch.TokenText != "true") {
                    branch.InsertCodeBeforeSelf("soccf.Gateway.branch(" + BranchCount + ", ");
                    branch.InsertCodeAfterSelf(")");
                    BranchCount++;
                }
            }
        }

        private IEnumerable<CstNode> FindLackingBlockNodes(CstNode root) {
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

        private IEnumerable<CstNode> FindStatementNodes(CstNode root) {
            return root.Descendants("statement")
                    .Where(e => {
                        // ブロック自身は意味を持たないステートメントで、中身だけが必要なので除外
                        if (e.FirstChild.Name == "block") {
                            return false;
                        }
                        // ラベルはループ文に付くため，ラベルの中身は除外
                        var second = e.Parent.Children().ElementAtOrDefault(1);
                        if (second != null && second.TokenText == ":"
                            && e.Parent.Name == "statement") {
                            return false;
                        }
                        if (e.FirstChild.TokenText == ";") {
                            return false;
                        }
                        return true;
                    });
        }

        private IEnumerable<CstNode> FindBranchNodes(CstNode root) {
            var ifWhileDoWhiles = root.Descendants("statement")
                    .Where(e => e.FirstChild.TokenText == "if"
                                || e.FirstChild.TokenText == "while"
                                || e.FirstChild.TokenText == "do")
                    .Select(e => e.Element("parExpression"))
                    .Select(e => e.Children().ElementAt(1));
            var fors = root.Descendants("forstatement")
                    .Where(e => e.Children().ElementAt(2).Name != "variableModifiers")
                    .SelectMany(e => e.Elements("expression"));
            var ternaries = root.Descendants("conditionalExpression")
                    .Where(e => e.Elements().Count() > 1)
                    .Select(e => e.FirstChild);
            return ifWhileDoWhiles.Concat(fors).Concat(ternaries);
        }
    }
}