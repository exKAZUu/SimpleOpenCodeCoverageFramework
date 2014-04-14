#region License

// Copyright (C) 2009-2012 Kazunori Sakamoto
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Collections.Generic;
using System.Linq;
using Code2Xml.Core.Generators;

namespace SimpleOccf {
    public static class JavaElements {
        public static IEnumerable<CstNode> Statement(CstNode root) {
            return root.Descendants("statement");
        }

        public static IEnumerable<CstNode> If(CstNode root) {
            return root.Descendants("statement")
                    .Where(e => e.FirstChild.TokenText == "if");
        }

        public static IEnumerable<CstNode> IfAndElseProcesses(CstNode root) {
            return root.Elements("statement");
        }

        public static IEnumerable<CstNode> While(CstNode root) {
            return root.Descendants("statement")
                    .Where(e => e.FirstChild.TokenText == "while");
        }

        public static CstNode WhileProcess(CstNode element) {
            return element.Children().ElementAt(2);
        }

        public static IEnumerable<CstNode> DoWhile(CstNode root) {
            return root.Descendants("statement")
                    .Where(e => e.FirstChild.TokenText == "do");
        }

        public static CstNode DoWhileProcess(CstNode element) {
            return element.Children().ElementAt(1);
        }

        public static IEnumerable<CstNode> For(CstNode root) {
            return root.Descendants("forstatement");
        }

        public static CstNode ForProcess(CstNode element) {
            return element.Child("statement");
        }
    }
}