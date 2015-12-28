using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using GitViz.Logic.Annotations;
using System;
using System.Text.RegularExpressions;
using GitViz.Logic.Mvvm.Utils;

namespace GitViz.Logic
{
    public class ViewModel : INotifyPropertyChanged
    {
        string _repositoryPath = "";
        CommitGraph _graph = new CommitGraph();
        RepositoryWatcher _watcher;

        readonly LogParser _parser = new LogParser();

        GitCommandExecutor commandExecutor;
        LogRetriever logRetriever;
        IWpfSystem _wpfSystem;

        /// <summary>
        /// TODO: need to inizialize everything with castle or other tools.
        /// </summary>
        public ViewModel()
        {
            _numOfCommitsToShow = 20;
            _wpfSystem = new WpfSystem();
        }

        public string WindowTitle
        {
            get
            {
                return "Readify GitViz (Alpha)"
                    + (string.IsNullOrWhiteSpace(_repositoryPath)
                        ? string.Empty
                        : " - " + Path.GetFileName(_repositoryPath));
            }
        }

        public bool IsNewRepository { get; set; }
        public string RepositoryPath
        {
            get { return _repositoryPath; }
            set
            {
                _repositoryPath = value.Trim();
                if (IsValidGitRepository(_repositoryPath))
                {
                     commandExecutor = new GitCommandExecutor(_repositoryPath);
                     logRetriever = new LogRetriever(commandExecutor, _parser);

                    RefreshGraph(logRetriever);
                    IsNewRepository = true;
                    _watcher = new RepositoryWatcher(_repositoryPath, IsBareGitRepository(_repositoryPath));
                    _watcher.ChangeDetected += (sender, args) => RefreshGraph(logRetriever);
					OnPropertyChanged("WindowTitle");
                }
                else
                {
                    Graph = new CommitGraph();
                    if (_watcher != null)
                    {
                        _watcher.Dispose();
                        _watcher = null;
                    }
                }
                OnPropertyChanged("RepositoryPath");
            }
        }

        void RefreshGraph(LogRetriever logRetriever)
        {
            var commits = logRetriever.GetRecentCommits(NumOfCommitsToShow).ToArray();
            var activeRefName = logRetriever.GetActiveReferenceName();

            var reachableCommitHashes = commits.Select(c => c.Hash).ToArray();
            var unreachableHashes = logRetriever.GetRecentUnreachableCommitHashes();
            IEnumerable<Commit> unreachableCommits = null;
            if (VisualizeUnreachable)
            {
                unreachableCommits = logRetriever
                .GetSpecificCommits(unreachableHashes)
                .Where(c => !reachableCommitHashes.Contains(c.Hash))
                .ToArray();
            }

            Graph = GenerateGraphFromCommits(commits, activeRefName, unreachableCommits);
        }

        CommitGraph GenerateGraphFromCommits(IEnumerable<Commit> commits, string activeRefName, IEnumerable<Commit> unreachableCommits)
        {
            commits = commits.ToList();

            var graph = new CommitGraph();
            List<Vertex> commitVertices;
            if (unreachableCommits != null)
            {
                commitVertices = commits.Select(c => new Vertex(c))
                .Union(unreachableCommits.Select(c => new Vertex(c) { Orphan = true }))
                .ToList();
            }
            else
            { 
            commitVertices = commits.Select(c => new Vertex(c))
                .ToList();
            }

            // Add all the vertices
            var headVertex = new Vertex(new Reference
            {
               Name = Reference.HEAD,
            });

            foreach (var commitVertex in commitVertices)
            {
                graph.AddVertex(commitVertex);

                if (commitVertex.Commit.Refs == null) continue;
                var isHeadHere = false;
                var isHeadSet = false;
                foreach (var refName in commitVertex.Commit.Refs)
                {
                    if (refName == Reference.HEAD)
                    {
                        isHeadHere = true;
                        graph.AddVertex(headVertex);
                        continue;
                    }
                    var refVertex = new Vertex(new Reference
                    {
                        Name = refName,
                        IsActive = refName == activeRefName
                    });
                    graph.AddVertex(refVertex);
                    graph.AddEdge(new CommitEdge(refVertex, commitVertex));
                    if (!refVertex.Reference.IsActive) continue;
                    isHeadSet = true;
                    graph.AddEdge(new CommitEdge(headVertex, refVertex));
                }
                if (isHeadHere && !isHeadSet)
                    graph.AddEdge(new CommitEdge(headVertex, commitVertex));
            }

            // Add all the edges
            foreach (var commitVertex in commitVertices.Where(c => c.Commit.ParentHashes != null))
            {
                foreach (var parentHash in commitVertex.Commit.ParentHashes)
                {
                    var parentVertex = commitVertices.SingleOrDefault(c => c.Commit.Hash == parentHash);
                    if (parentVertex != null) graph.AddEdge(new CommitEdge(commitVertex, parentVertex));
                }
            }

            return graph;
        }

        public CommitGraph Graph
        {
            get { return _graph; }
            set 
            {
                _graph = value;
                OnPropertyChanged("Graph");
            }
        }

        public Boolean VisualizeUnreachable
        {
            get { return _visualizeUnreachable; }
            set
            {
                _visualizeUnreachable = value;
                if (logRetriever != null) RefreshGraph(logRetriever); //TODO: Refactor.
                OnPropertyChanged("VisualizeUnreachable");
            }
        }
        private Boolean _visualizeUnreachable;

        public Boolean VisualizeComments
        {
            get { return _visualizeComments; }
            set
            {
                _visualizeComments = value;
                if (logRetriever != null) RefreshGraph(logRetriever); //TODO: Refactor.
                OnPropertyChanged("VisualizeComments");
            }
        }
        private Boolean _visualizeComments;

        public Int32 NumOfCommitsToShow
        {
            get { return _numOfCommitsToShow; }
            set
            {
                _numOfCommitsToShow = value;
                if (logRetriever != null) RefreshGraph(logRetriever); //TODO: Refactor.
                OnPropertyChanged("NumOfCommitsToShow");
            }
        }
        private Int32 _numOfCommitsToShow;

        public bool VisualizeCommitDate
        {
            get { return _visualizeCommitDate; }
            set
            {
                _visualizeCommitDate = value;
                if (logRetriever != null) RefreshGraph(logRetriever); //TODO: Refactor.
                OnPropertyChanged("VisualizeCommitDate");
            }
        }

        private bool _visualizeCommitDate;

        static bool IsValidGitRepository(string path)
        {
            return !string.IsNullOrEmpty(path)
                && Directory.Exists(path)
                && (Directory.Exists(Path.Combine(path, ".git")) ||
                 IsBareGitRepository(path));
        }

        static Boolean IsBareGitRepository(String path) 
        {
            String configFileForBareRepository = Path.Combine(path, "config"); 
            return File.Exists(configFileForBareRepository) &&
                  Regex.IsMatch(File.ReadAllText(configFileForBareRepository), @"bare\s*=\s*true", RegexOptions.IgnoreCase);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Commands 

        public void ExecuteSelectFolder(Object paramter) 
        {
            var folder = _wpfSystem.UserChooseFolder(RepositoryPath);
            if (!String.IsNullOrEmpty(folder)) 
            {
                RepositoryPath = folder;
            }
        }


        #endregion 
    }
}
