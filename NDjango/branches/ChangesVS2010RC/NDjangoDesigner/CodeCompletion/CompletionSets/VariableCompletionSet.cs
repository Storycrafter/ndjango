﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class VariableCompletionSet : AbstractCompletionSet
    {
        public VariableCompletionSet() { }

        public override void SelectBestMatch()
        {
            SelectionStatus = new CompletionSelectionStatus(Completions[0], false, true);
        }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "{ ", "");
        }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(new List<string>(new string[] { " }}" }));
        }
    }
}
