﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PM = System.Tuple<System.Text.RegularExpressions.Regex, System.Action<System.Text.RegularExpressions.Match>>;

namespace Docdown.Controls.Markdown
{
    public static class ListInputHandler
    {
        public static readonly Regex UnorderedListCheckboxPattern = new Regex(@"^[ ]{0,3}[-\*\+][ ]{1,3}\[[ xX]\](?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListCheckboxEndPattern = new Regex(@"^[ ]{0,3}[-\*\+][ ]{1,3}\[[ xX]\]\s*", RegexOptions.Compiled);
        public static readonly Regex OrderedListPattern = new Regex(@"^[ ]{0,3}(\d+)\.(?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex OrderedListEndPattern = new Regex(@"^[ ]{0,3}(\d+)\.(?=[ ]{1,3}\s*)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListPattern = new Regex(@"^[ ]{0,3}[-\*\+](?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListEndPattern = new Regex(@"^[ ]{0,3}[-\*\+](?=[ ]{1,3}\s*)", RegexOptions.Compiled);
        public static readonly Regex BlockQuotePattern = new Regex(@"^(([ ]{0,4}>)+)[ ]{0,4}.{2}", RegexOptions.Compiled);
        public static readonly Regex BlockQuoteEndPattern = new Regex(@"^([ ]{0,4}>)+[ ]{0,4}\s*", RegexOptions.Compiled);

        public static void AdjustList(TextEditor editor, bool lineCountIncreased)
        {
            var document = editor.Document;
            var line = document.GetLineByOffset(editor.SelectionStart)?.PreviousLine;
            if (line == null) return;
            var text = document.GetText(line.Offset, line.Length);

            // A poor mans pattern matcher

            void match(string txt, IEnumerable<PM> patterns)
            {
                var firstMatch = patterns
                    .Select(pattern => new { Match = pattern.Item1.Match(txt), Action = pattern.Item2 })
                    .FirstOrDefault(ma => ma.Match.Success);

                firstMatch?.Action(firstMatch.Match);
            }

            void ifIncreased(Action func)
            {
                if (lineCountIncreased) func();
            }

            void atEndOfList(string symbol, Action action)
            {
                var previous = line.PreviousLine;
                if (previous == null || previous.Length == 0) return;
                var previousText = document.GetText(previous.Offset, previous.Length);
                if (previousText.StartsWith(symbol)) action();
            }

            document.BeginUpdate();

            match(text, new[]
            {
                new PM(UnorderedListCheckboxPattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart() + " "))),

                new PM(UnorderedListCheckboxEndPattern,
                    m => atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(UnorderedListPattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart() + " "))),

                new PM(UnorderedListEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(OrderedListPattern, m =>
                {
                    var number = int.Parse(m.Groups[1].Value);
                    if (lineCountIncreased)
                    {
                        number += 1;
                        document.Insert(editor.SelectionStart, number + ". ");
                        line = line.NextLine;
                    }
                    RenumberOrderedList(document, line, number);
                }),

                new PM(OrderedListEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(BlockQuotePattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[1].Value.TrimStart()))),

                new PM(BlockQuoteEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line)))
            });

            document.EndUpdate();
        }

        private static void RenumberOrderedList(
            IDocument document,
            DocumentLine line,
            int number)
        {
            if (line == null) return;
            while ((line = line.NextLine) != null)
            {
                number += 1;
                var text = document.GetText(line.Offset, line.Length);
                var match = OrderedListPattern.Match(text);
                if (match.Success == false) break;
                var group = match.Groups[1];
                var currentNumber = int.Parse(group.Value);
                if (currentNumber != number)
                {
                    document.Replace(line.Offset + group.Index, group.Length, number.ToString());
                }
            }
        }
    }
}
