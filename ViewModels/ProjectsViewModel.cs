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

        private int m_selectedProjectIndex;
        public int SelectedProjectIndex
        {
            get { return m_selectedProjectIndex; }
            set
            {
                m_selectedProjectIndex = value;
                RaiseEvent();
                SelectedProjectChanged();
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
            m_selectedProjectIndex = -1;
        }

        private void PopulateProjects()
        {
            if (SessionManager.Adapter == null)
                return;

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
        private void SelectedProjectChanged()
        {
            SessionManager.Instance.SelectedProject = m_projects[SelectedProjectIndex];
            SessionManager.Instance.StageSwitch(2);
        }
    }
}
