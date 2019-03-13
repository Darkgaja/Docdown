﻿using PandocMark.Syntax;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Docdown.Controls.Markdown
{
    public class BlockBackgroundRenderer : IBackgroundRenderer
    {
        private readonly BlockTag[] _blockTags =
        {
            BlockTag.FencedCode,
            BlockTag.IndentedCode,
            BlockTag.AtxHeading,
            BlockTag.SetextHeading,
            BlockTag.HtmlBlock,
            BlockTag.BlockQuote
        };

        private readonly Dictionary<BlockTag, Brush> _brushes = new Dictionary<BlockTag, Brush>();
        private Block _abstractSyntaxTree;

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            var ast = _abstractSyntaxTree;
            if (ast is null) return;

            foreach (var line in textView.VisualLines)
            {
                var rc = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, line, 0, 1000).FirstOrDefault();
                if (rc == default) continue;

                foreach (var block in AbstractSyntaxTree.EnumerateBlocks(ast.FirstChild))
                {
                    if (block.SourcePosition + block.SourceLength <= line.StartOffset) continue;
                    if (block.SourcePosition > line.LastDocumentLine.EndOffset) break;

                    // Indented code does not terminate (at least in the AST) until a new
                    // block is encountered. Thus blank lines at the end of the block are
                    // highlighted. It looks much better if they're not highlighted.
                    if (block.Tag == BlockTag.IndentedCode)
                    {
                        var length = block.SourcePosition + block.SourceLength - line.StartOffset;
                        if (line.StartOffset + length > textView.Document.TextLength) break;
                        var remainder = textView.Document.GetText(line.StartOffset, length);
                        if (string.IsNullOrWhiteSpace(remainder)) break;
                    }

                    if (_blockTags.Any(tag => tag == block.Tag))
                    {
                        if (_brushes.TryGetValue(block.Tag, out Brush brush) && brush != null)
                        {
                            drawingContext.DrawRectangle(brush, null,
                                new Rect(0, rc.Top, textView.ActualWidth, line.Height));
                        }
                    }
                }
            }
        }

        public void UpdateAbstractSyntaxTree(Block ast) { _abstractSyntaxTree = ast; }

        public void OnThemeChanged(Theme theme)
        {
            _brushes.Clear();
            var codeBrush = ColorBrush(theme.HighlightBlockCode.Background);
            _brushes[BlockTag.FencedCode] = codeBrush;
            _brushes[BlockTag.IndentedCode] = codeBrush;
            _brushes[BlockTag.HtmlBlock] = codeBrush;

            var headingBrush = ColorBrush(theme.HighlightHeading.Background);
            _brushes[BlockTag.AtxHeading] = headingBrush;
            _brushes[BlockTag.SetextHeading] = headingBrush;

            _brushes[BlockTag.BlockQuote] = ColorBrush(theme.HighlightBlockQuote.Background);
        }

        private static Brush ColorBrush(string color)
        {
            if (string.IsNullOrWhiteSpace(color)) return null;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                brush.Freeze();
                return brush;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
