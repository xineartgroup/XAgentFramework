using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XAgentFramework
{
    public enum MarkdownBlockType
    {
        Paragraph,
        Header1,
        Header2,
        Header3,
        BulletItem,
        Table,
        CodeBlock
    }

    public class MarkdownInlineToken
    {
        public string Text { get; set; } = string.Empty;
        public bool IsBold { get; set; }
        public bool IsCode { get; set; }
        public bool IsLink { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class MarkdownTableBlock
    {
        public List<string> Headers { get; set; } = [];
        public List<List<List<MarkdownInlineToken>>> Rows { get; set; } = [];
    }

    public class MarkdownBlock
    {
        public MarkdownBlockType Type { get; set; }
        public List<MarkdownInlineToken> Tokens { get; set; } = [];
        public MarkdownTableBlock? TableData { get; set; }
        public string RawCode { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }

    public class RenderedHyperlink
    {
        public Rectangle Bounds { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public static class MarkdownParser
    {
        public static List<MarkdownBlock> Parse(string content)
        {
            var blocks = new List<MarkdownBlock>();
            if (string.IsNullOrWhiteSpace(content)) return blocks;

            string cleaned = content.Replace("\\u0022", "\"").Replace("\\\"", "\"");
            string[] lines = cleaned.Replace("\r\n", "\n").Split('\n');

            int i = 0;
            while (i < lines.Length)
            {
                string line = lines[i];
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed))
                {
                    i++;
                    continue;
                }

                // 1. Code Block Detection (```)
                if (trimmed.StartsWith("```"))
                {
                    string language = trimmed.Length > 3 ? trimmed[3..].Trim() : string.Empty;
                    i++;
                    var codeLines = new List<string>();

                    while (i < lines.Length && !lines[i].Trim().StartsWith("```"))
                    {
                        codeLines.Add(lines[i]);
                        i++;
                    }

                    if (i < lines.Length && lines[i].Trim().StartsWith("```"))
                    {
                        i++; // Skip closing ```
                    }

                    blocks.Add(new MarkdownBlock
                    {
                        Type = MarkdownBlockType.CodeBlock,
                        Language = language,
                        RawCode = string.Join("\n", codeLines)
                    });
                    continue;
                }

                // 2. Table Detection
                if (trimmed.StartsWith('|') && trimmed.EndsWith('|'))
                {
                    var tableBlock = ParseTable(lines, ref i);
                    if (tableBlock != null)
                    {
                        blocks.Add(tableBlock);
                        continue;
                    }
                }

                // 3. Line Block Types
                MarkdownBlock block = new();

                if (trimmed.StartsWith("### "))
                {
                    block.Type = MarkdownBlockType.Header3;
                    trimmed = trimmed[4..];
                }
                else if (trimmed.StartsWith("## "))
                {
                    block.Type = MarkdownBlockType.Header2;
                    trimmed = trimmed[3..];
                }
                else if (trimmed.StartsWith("# "))
                {
                    block.Type = MarkdownBlockType.Header1;
                    trimmed = trimmed[2..];
                }
                else if (trimmed.StartsWith("* ") || trimmed.StartsWith("- "))
                {
                    block.Type = MarkdownBlockType.BulletItem;
                    trimmed = trimmed[2..];
                }
                else
                {
                    block.Type = MarkdownBlockType.Paragraph;
                }

                block.Tokens = ParseInlineTokens(trimmed);
                blocks.Add(block);
                i++;
            }

            return blocks;
        }

        private static MarkdownBlock? ParseTable(string[] lines, ref int index)
        {
            var table = new MarkdownTableBlock();

            string headerLine = lines[index].Trim();
            table.Headers = [.. headerLine.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim())];
            index++;

            if (index < lines.Length && lines[index].Trim().StartsWith('|') && lines[index].Contains("---"))
            {
                index++;
            }

            while (index < lines.Length)
            {
                string rowLine = lines[index].Trim();
                if (!rowLine.StartsWith('|') || !rowLine.EndsWith('|')) break;

                string[] rawCells = rowLine.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var rowCells = new List<List<MarkdownInlineToken>>();

                foreach (string cell in rawCells)
                {
                    rowCells.Add(ParseInlineTokens(cell.Trim()));
                }

                table.Rows.Add(rowCells);
                index++;
            }

            return new MarkdownBlock
            {
                Type = MarkdownBlockType.Table,
                TableData = table
            };
        }

        public static List<MarkdownInlineToken> ParseInlineTokens(string text)
        {
            var tokens = new List<MarkdownInlineToken>();

            // Pattern captures: [Label](Url), **Bold**, or `Code`
            string pattern = @"(\[.*?\]\(.*?\))|(\*\*.*?\*\*)|(`.*?`)";
            string[] parts = Regex.Split(text, pattern);

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                // Hyperlink [Label](Url)
                if (part.StartsWith('[') && part.Contains("](") && part.EndsWith(')'))
                {
                    int linkTextEnd = part.IndexOf("](");
                    string label = part[1..linkTextEnd];
                    string url = part[(linkTextEnd + 2)..^1];

                    tokens.Add(new MarkdownInlineToken
                    {
                        Text = label,
                        IsLink = true,
                        Url = url
                    });
                }
                // Inline Bold **Text**
                else if (part.StartsWith("**") && part.EndsWith("**") && part.Length >= 4)
                {
                    tokens.Add(new MarkdownInlineToken
                    {
                        Text = part[2..^2],
                        IsBold = true
                    });
                }
                // Inline Code `code`
                else if (part.StartsWith('`') && part.EndsWith('`') && part.Length >= 2)
                {
                    tokens.Add(new MarkdownInlineToken
                    {
                        Text = part[1..^1],
                        IsCode = true
                    });
                }
                else
                {
                    tokens.Add(new MarkdownInlineToken
                    {
                        Text = part,
                        IsBold = false,
                        IsCode = false,
                        IsLink = false
                    });
                }
            }

            return tokens;
        }
    }

    public static class MiniMarkdownRenderer
    {
        public static Size MeasureMarkdown(Graphics g, string text, Font baseFont, int maxWidth)
        {
            var blocks = MarkdownParser.Parse(text);
            int currentY = 0;
            int maxMeasuredWidth = 0;

            foreach (var block in blocks)
            {
                if (block.Type == MarkdownBlockType.Table && block.TableData != null)
                {
                    Size tableSize = MeasureTable(g, block.TableData, baseFont, maxWidth);
                    maxMeasuredWidth = Math.Max(maxMeasuredWidth, tableSize.Width);
                    currentY += tableSize.Height + 6;
                    continue;
                }

                if (block.Type == MarkdownBlockType.CodeBlock)
                {
                    Size codeBlockSize = MeasureCodeBlock(g, block.RawCode, maxWidth);
                    maxMeasuredWidth = Math.Max(maxMeasuredWidth, codeBlockSize.Width);
                    currentY += codeBlockSize.Height + 6;
                    continue;
                }

                using Font blockFont = GetBlockBaseFont(baseFont, block.Type);
                int indent = block.Type == MarkdownBlockType.BulletItem ? 16 : 0;
                int availableWidth = Math.Max(20, maxWidth - indent);

                Size blockSize = MeasureOrDrawBlock(g, block, blockFont, availableWidth, 0, currentY, draw: false, renderedLinks: null);
                maxMeasuredWidth = Math.Max(maxMeasuredWidth, blockSize.Width + indent);
                currentY += blockSize.Height + 4;
            }

            return new Size(maxMeasuredWidth, Math.Max(currentY, 16));
        }

        public static List<RenderedHyperlink> DrawMarkdown(Graphics g, string text, Font baseFont, Color textColor, Rectangle bounds)
        {
            var renderedLinks = new List<RenderedHyperlink>();
            var blocks = MarkdownParser.Parse(text);
            int currentY = bounds.Y;

            foreach (var block in blocks)
            {
                if (currentY >= bounds.Bottom) break;

                if (block.Type == MarkdownBlockType.Table && block.TableData != null)
                {
                    Size tableSize = MeasureTable(g, block.TableData, baseFont, bounds.Width);
                    Rectangle tableBounds = new(bounds.X, currentY, bounds.Width, tableSize.Height);

                    DrawTable(g, block.TableData, baseFont, textColor, tableBounds, renderedLinks);
                    currentY += tableSize.Height + 6;
                    continue;
                }

                if (block.Type == MarkdownBlockType.CodeBlock)
                {
                    Size codeBlockSize = MeasureCodeBlock(g, block.RawCode, bounds.Width);
                    Rectangle codeBounds = new(bounds.X, currentY, bounds.Width, codeBlockSize.Height);

                    DrawCodeBlock(g, block.RawCode, codeBounds);
                    currentY += codeBlockSize.Height + 6;
                    continue;
                }

                using Font blockFont = GetBlockBaseFont(baseFont, block.Type);
                int indent = block.Type == MarkdownBlockType.BulletItem ? 16 : 0;
                int availableWidth = bounds.Width - indent;

                if (block.Type == MarkdownBlockType.BulletItem)
                {
                    using Brush bulletBrush = new SolidBrush(textColor);
                    g.FillEllipse(bulletBrush, bounds.X + 4, currentY + (blockFont.Height / 2) - 2, 4, 4);
                }

                Size blockSize = MeasureOrDrawBlock(g, block, blockFont, availableWidth, bounds.X + indent, currentY, draw: true, color: textColor, renderedLinks: renderedLinks);
                currentY += blockSize.Height + 4;
            }

            return renderedLinks;
        }

        private static Font GetBlockBaseFont(Font baseFont, MarkdownBlockType type)
        {
            return type switch
            {
                MarkdownBlockType.Header1 => new Font(baseFont.FontFamily, baseFont.Size + 5, FontStyle.Bold),
                MarkdownBlockType.Header2 => new Font(baseFont.FontFamily, baseFont.Size + 3, FontStyle.Bold),
                MarkdownBlockType.Header3 => new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold),
                _ => new Font(baseFont, baseFont.Style)
            };
        }

        private static Size MeasureOrDrawBlock(Graphics g, MarkdownBlock block, Font font, int maxWidth, int x, int y, bool draw, Color? color = null, List<RenderedHyperlink>? renderedLinks = null)
        {
            int currentX = x;
            int currentY = y;
            int lineHeight = font.Height + 2;
            int maxRowWidth = 0;

            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            foreach (var token in block.Tokens)
            {
                FontStyle style = font.Style;
                if (token.IsBold) style |= FontStyle.Bold;
                if (token.IsLink) style |= FontStyle.Underline;

                string fontFamily = token.IsCode ? "Consolas" : font.FontFamily.Name;
                float fontSize = token.IsCode ? font.Size - 1f : font.Size;

                using Font tokenFont = new(fontFamily, Math.Max(fontSize, 8f), style);
                Color tokenColor = token.IsLink ? Color.RoyalBlue : (color ?? Color.Black);

                string[] words = token.Text.Split(' ');

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];
                    if (i < words.Length - 1) word += " ";

                    Size wordSize = TextRenderer.MeasureText(g, word, tokenFont, new Size(maxWidth, lineHeight), flags);

                    if (currentX + wordSize.Width > x + maxWidth && currentX > x)
                    {
                        currentX = x;
                        currentY += lineHeight;
                    }

                    if (draw)
                    {
                        Rectangle wordRect = new(currentX, currentY, wordSize.Width, lineHeight);

                        // Draw background for inline code (`code`)
                        if (token.IsCode)
                        {
                            using Brush bgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                            g.FillRectangle(bgBrush, wordRect);
                        }

                        TextRenderer.DrawText(g, word, tokenFont, new Point(currentX, currentY), tokenColor, flags);

                        // Capture URL click area
                        if (token.IsLink && renderedLinks != null && !string.IsNullOrEmpty(token.Url))
                        {
                            renderedLinks.Add(new RenderedHyperlink
                            {
                                Bounds = wordRect,
                                Url = token.Url
                            });
                        }
                    }

                    currentX += wordSize.Width;
                    maxRowWidth = Math.Max(maxRowWidth, currentX - x);
                }
            }

            int totalHeight = (currentY - y) + lineHeight;
            return new Size(maxRowWidth, totalHeight);
        }

        private static Size MeasureCodeBlock(Graphics g, string code, int maxWidth)
        {
            using Font codeFont = new("Consolas", 9f);
            string[] lines = code.Split('\n');
            int lineHeight = codeFont.Height + 2;
            int padding = 12;

            int height = (lines.Length * lineHeight) + (padding * 2);
            return new Size(maxWidth, height);
        }

        private static void DrawCodeBlock(Graphics g, string code, Rectangle bounds)
        {
            using Font codeFont = new("Consolas", 9f);
            using Brush bgBrush = new SolidBrush(Color.FromArgb(245, 245, 245));
            using Pen borderPen = new(Color.FromArgb(210, 210, 210), 1f);

            // Fill code box background and draw outline
            g.FillRectangle(bgBrush, bounds);
            g.DrawRectangle(borderPen, bounds);

            string[] lines = code.Split('\n');
            int currentY = bounds.Y + 8;
            int lineHeight = codeFont.Height + 2;

            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            foreach (string line in lines)
            {
                TextRenderer.DrawText(g, line, codeFont, new Point(bounds.X + 8, currentY), Color.FromArgb(40, 40, 40), flags);
                currentY += lineHeight;

                if (currentY + lineHeight > bounds.Bottom) break;
            }
        }

        private static Size MeasureTable(Graphics g, MarkdownTableBlock table, Font font, int totalWidth)
        {
            int colCount = table.Headers.Count;
            if (colCount == 0) return Size.Empty;

            int rowHeight = font.Height + 10;
            int totalRows = 1 + table.Rows.Count;
            int tableHeight = totalRows * rowHeight;

            return new Size(totalWidth, tableHeight);
        }

        private static void DrawTable(Graphics g, MarkdownTableBlock table, Font font, Color textColor, Rectangle bounds, List<RenderedHyperlink> renderedLinks)
        {
            int colCount = table.Headers.Count;
            if (colCount == 0) return;

            int colWidth = bounds.Width / colCount;
            int rowHeight = font.Height + 10;
            int currentY = bounds.Y;

            using Pen borderPen = new(Color.LightGray, 1f);
            using Font headerFont = new(font, FontStyle.Bold);

            // 1. Draw Headers
            for (int col = 0; col < colCount; col++)
            {
                Rectangle cellRect = new(bounds.X + (col * colWidth), currentY, colWidth, rowHeight);
                g.DrawRectangle(borderPen, cellRect);

                Rectangle textRect = new(cellRect.X + 4, cellRect.Y + 3, cellRect.Width - 8, cellRect.Height - 6);
                TextRenderer.DrawText(g, table.Headers[col], headerFont, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }

            currentY += rowHeight;

            // 2. Draw Data Rows
            foreach (var row in table.Rows)
            {
                for (int col = 0; col < Math.Min(colCount, row.Count); col++)
                {
                    Rectangle cellRect = new(bounds.X + (col * colWidth), currentY, colWidth, rowHeight);
                    g.DrawRectangle(borderPen, cellRect);

                    Rectangle textRect = new(cellRect.X + 4, cellRect.Y + 3, cellRect.Width - 8, cellRect.Height - 6);
                    DrawInlineTokensInRect(g, row[col], font, textRect, textColor, renderedLinks);
                }
                currentY += rowHeight;
            }
        }

        private static void DrawInlineTokensInRect(Graphics g, List<MarkdownInlineToken> tokens, Font font, Rectangle bounds, Color color, List<RenderedHyperlink> renderedLinks)
        {
            int currentX = bounds.X;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            foreach (var token in tokens)
            {
                FontStyle style = font.Style;
                if (token.IsBold) style |= FontStyle.Bold;
                if (token.IsLink) style |= FontStyle.Underline;

                string fontFamily = token.IsCode ? "Consolas" : font.FontFamily.Name;
                float fontSize = token.IsCode ? font.Size - 1f : font.Size;

                using Font tokenFont = new(fontFamily, Math.Max(fontSize, 8f), style);
                Color tokenColor = token.IsLink ? Color.RoyalBlue : color;

                Size tokenSize = TextRenderer.MeasureText(g, token.Text, tokenFont, bounds.Size, flags);

                if (currentX + tokenSize.Width > bounds.Right)
                {
                    flags |= TextFormatFlags.EndEllipsis;
                }

                Rectangle tokenBounds = new(currentX, bounds.Y, tokenSize.Width, bounds.Height);

                TextRenderer.DrawText(g, token.Text, tokenFont, new Point(currentX, bounds.Y), tokenColor, flags);

                if (token.IsLink && !string.IsNullOrEmpty(token.Url))
                {
                    renderedLinks.Add(new RenderedHyperlink
                    {
                        Bounds = tokenBounds,
                        Url = token.Url
                    });
                }

                currentX += tokenSize.Width;
                if (currentX >= bounds.Right) break;
            }
        }

        /// <summary>
        /// Helper to check if a mouse click hit any hyperlink rendered on screen.
        /// </summary>
        public static string? GetClickedUrl(List<RenderedHyperlink> links, Point clickLocation)
        {
            foreach (var link in links)
            {
                if (link.Bounds.Contains(clickLocation))
                {
                    return link.Url;
                }
            }
            return null;
        }
    }
}