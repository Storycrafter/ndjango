﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using System.Threading;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides ability to retrive tokens out of snapshot objects.
    /// </summary>
    class NodeProvider
    {
        // it can take some time for the parser to build the token list.
        // for now let us initialize it to an empty list
        private List<NodeSnapshot> tokens = new List<NodeSnapshot>();
        
        private object token_lock = new object();
        private IParser parser;
        private ITextBuffer buffer;

        public NodeProvider(IParser parser, ITextBuffer buffer)
        {
            this.parser = parser;
            this.buffer = buffer;
            rebuildNodes(buffer.CurrentSnapshot);
            buffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
        }

        public delegate void SnapshotEvent (SnapshotSpan snapshotSpan);

        void buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            rebuildNodes(e.After);
        }

        private void rebuildNodes(ITextSnapshot snapshot)
        {
            ThreadPool.QueueUserWorkItem(rebuildNodesAsynch, snapshot);
        }

        public event SnapshotEvent NodesChanged;

        /// <summary>
        /// Retrieves sequence of tokens out of snapshot object. 
        /// </summary>
        private void rebuildNodesAsynch(object snapshotObject)
        {
            ITextSnapshot snapshot = (ITextSnapshot)snapshotObject;
            List<NodeSnapshot> tokens = parser.Parse(snapshot.Lines.ToList().ConvertAll(line => line.GetTextIncludingLineBreak()))
                .ToList()
                    .ConvertAll<NodeSnapshot>
                        (token => new NodeSnapshot(snapshot, token));
            lock (token_lock)
            {
                this.tokens = tokens;
            }
            if (NodesChanged != null)
                NodesChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        internal List<NodeSnapshot> GetNodes(SnapshotSpan snapshotSpan)
        {
            List<NodeSnapshot> tokens;
            lock (token_lock)
            {
                tokens = this.tokens;
            }
            if (tokens.Count == 0)
                return tokens;
            
            // just in case if while the tokens list was being rebuilt
            // another modification was made
            if (this.tokens[0].SnapshotSpan.Snapshot != snapshotSpan.Snapshot)
                this.tokens.ForEach(token => token.TranslateTo(snapshotSpan.Snapshot));

            return tokens;
        }
        
        /// <summary>
        /// Gets a list of intellisense values of selected token.
        /// </summary>
        /// <param name="point">Mouse cursor destination</param>
        /// <returns></returns>
        internal List<string> GetCompletions(SnapshotPoint point)
        {
            NodeSnapshot result = GetNodes(new SnapshotSpan(point.Snapshot, point.Position, 0))
                .FirstOrDefault(token => token.SnapshotSpan.IntersectsWith(new SnapshotSpan(point.Snapshot, point.Position, 0)));
            if (result == null)
                return new List<string>();
            return new List<string>(result.Node.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal INode GetQuickInfo(SnapshotPoint point)
        {
            NodeSnapshot result = GetNodes(new SnapshotSpan(point.Snapshot, point.Position, 0))
                            .FirstOrDefault(token => token.SnapshotSpan.IntersectsWith(new SnapshotSpan(point.Snapshot, point.Position, 0)));
            if (result == null)
                return null;
            return result.Node;
        }
    }
}
