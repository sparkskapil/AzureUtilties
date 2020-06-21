using AzureUtilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViewModels.Helper;

namespace ViewModels
{
    public class ProjectsViewModel : ViewModelBase
    {
        private ObservableCollection<TeamProject> m_projects;
        private bool m_isBusy;

        public bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                m_isBusy = value;
                RaiseEvent();
            }
        }

        public ObservableCollection<TeamProject> Projects
        {
            get { return m_projects; }
            set { m_projects = value; }
        }

        public ProjectsViewModel()
        {
            IsBusy = false;
            Projects = new ObservableCollection<TeamProject>();
            PopulateProjects();
            SessionManager.Instance.StageSwitchEvent += new StageSwitchHandler(StageSwitched);
        }

        private void PopulateProjects()
        {
            var uiContext = SynchronizationContext.Current;
            IsBusy = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += delegate { IsBusy = false; };
            worker.DoWork += delegate
            {
                var projects = SessionManager.Adapter.Projects.GetAllProjects();
                foreach (TeamProject project in projects)
                {
                    uiContext.Send(x => Projects.Add(project), null);
                }
            };

            worker.RunWorkerAsync();
        }

        protected void StageSwitched(int index)
        {
            if (index == 1)
            {
                PopulateProjects();
            }
        }
    }
}
