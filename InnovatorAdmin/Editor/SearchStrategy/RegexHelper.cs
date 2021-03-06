﻿using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace InnovatorAdmin.Editor
{
  public class RegexHelper : PlainTextEditorHelper
  {
    private InsightWindow _currentInsight;

    public override string BlockCommentEnd { get { return ")"; } }
    public override string BlockCommentStart { get { return "(?#"; } }
    public override string LineComment { get { return "#"; } }

    public override ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition GetHighlighting()
    {
      return _highlighter;
    }

    public override void HandleTextEntered(EditorWinForm control, string insertText)
    {
      switch (insertText)
      {
        case "\\":
          ShowCompletions(control);
          break;
        case "{":
        case "(":
        case "[":
          if (_currentInsight != null)
            _currentInsight.Hide();

          var overload = new OverloadInsightWindow(control.Editor.TextArea);
          if (insertText == "{")
            overload.Provider = _quantifiers;
          else if (insertText == "(")
            overload.Provider = _groups;
          else
            overload.Provider = _charClass;
          overload.Show();
          _currentInsight = overload;
          break;
        case "}":
        case ")":
        case "]":
          if (_currentInsight != null)
            _currentInsight.Hide();

          _currentInsight = null;
          break;
      }
    }

    private static OverloadList _quantifiers = new OverloadList()
    {
      {"{n}", "Exactly n times"},
      {"{n,}", "At least n times"},
      {"{n,}?", "At least n times as few times as possible"},
      {"{n,m}", "At least n, but no more than m times"},
      {"{n,m}?", "At least n, but no more than m times as few times as possible"}
    };

    private static OverloadList _groups = new OverloadList()
    {
      {"(subexpression)", "Captures the matched subexpression and assigns it a one-based ordinal number."},
      {"(?: subexpression)", "Defines a noncapturing group."},
      {"(?= subexpression)", "Zero-width positive lookahead assertion."},
      {"(?! subexpression)", "Zero-width negative lookahead assertion."},
      {"(?<= subexpression)", "Zero-width positive lookbehind assertion."},
      {"(?> subexpression)", "Nonbacktracking (or \"greedy\") subexpression."},
      {"(?# comment)", "Inline comment"}
    };

    private static OverloadList _charClass = new OverloadList()
    {
      {"[character_group]", "Matches the listed characters."},
      {"[^character_group]", "Matches characters which aren't listed."}
    };

    private static string[][] _completionInfo = new string[][] {
      new string[] { "a", "Bell Character (\\u0007)"},
      new string[] { "b", "Backspace (in a character class) (\\u0008)"},
      new string[] { "t", "Tab (\\u0009)"},
      new string[] { "r", "Carriage Return (\\u000d)"},
      new string[] { "v", "Vertical Tab (\\u000b)"},
      new string[] { "f", "Form Feed (\\u000c)"},
      new string[] { "n", "New Line (\\u000a)"},
      new string[] { "e", "Escape (\\u001b)"},
      new string[] { "w", "Word character ([a-zA-Z0-9_])"},
      new string[] { "W", "Non-word character ([^a-zA-Z0-9_])"},
      new string[] { "s", "White-space character"},
      new string[] { "S", "Non-white-space character"},
      new string[] { "d", "Digit ([0-9])"},
      new string[] { "D", "Non-digit ([^0-9])"},
      new string[] { "A", "Match occurs at the start of the string"},
      new string[] { "Z", "Match occurs at the end of the string or line"},
      new string[] { "z", "Match occurs at the end of the string or line"},
      new string[] { "G", "Match occurs where previous match ended"},
      new string[] { "b", "Match occurs on a boundary between a \\w and \\W character"},
      new string[] { "B", "Match does not occur on a \\b boundary"}
    };

    public override Innovator.Client.IPromise<CompletionContext> ShowCompletions(EditorWinForm control)
    {
      var length = control.Editor.Document.TextLength;
      var caret = control.Editor.CaretOffset;
      var data = new CompletionContext();

      if (caret > 0 && control.Editor.Document.GetCharAt(caret - 1) == '\\')
      {
        data.Items = _completionInfo
          .OrderBy(i => i[0].ToLowerInvariant())
          .ThenBy(i => i[1])
          .Select(i => new BasicCompletionData() {
            Content = FormatText.Span(FormatText.Text(i[0] + " "), FormatText.MutedText(i[1])),
            Text = i[0],
            Action = () => i[0]
          });
      }

      if (data.Items.Any())
      {
        var items = data.Items.ToArray();
        var contextItems = items.OfType<IContextCompletions>();
        foreach (var contextItem in contextItems)
        {
          contextItem.SetContext(this, control);
        }

        control.ShowCompletionWindow(items, data.Overlap);
      }

      return Promises.Resolved(data);
    }

    internal static IHighlightingDefinition _highlighter;

    static RegexHelper()
    {
      using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("InnovatorAdmin.resources.Regex.xshd"))
      {
        using (var reader = new System.Xml.XmlTextReader(stream))
        {
          _highlighter =
              ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
              ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
        }
      }
    }
  }
}
