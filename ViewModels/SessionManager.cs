using AzureUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    class SessionManager
    {
        AzureAdapter m_adapter;
        public TeamProject? SelectedProject { get; private set; }
        public BuildPipeline? SelectedPipeline { get; private set; }
        public BuildInfo? SelectedBuild { get; private set; }
        public TestRun? SelectedTestRun { get; private set; }
        protected SessionManager()
        {
            m_adapter = null;
            ResetSelectables();
        }

        public static SessionManager Instance
        {
            get
            {
                if(m_instance == null)  m_instance = new SessionManager();
                return m_instance;
            }
        }
        public static AzureAdapter Adapter 
        { 
            get
            {
                return SessionManager.Instance.m_adapter;
            }
        }

        private void ResetSelectables()
        {

            SelectedProject = null;
            SelectedPipeline = null;
            SelectedBuild = null;
            SelectedTestRun = null;
        }

        public void Reset(string url)
        {
            m_adapter = new AzureAdapter(url);
            ResetSelectables();
        }

        // This method can be used to load previous session
        // From a file or database
        public void LoadSession()
        {

        }
        static SessionManager m_instance;
    }
}
