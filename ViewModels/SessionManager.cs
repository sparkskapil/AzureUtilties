﻿using AzureUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public delegate void StageSwitchHandler(int index);

    public enum NavigationStage
    {
        Connect,
        Projects,
        Pipelines,
        TestRuns
    };
    public class SessionManager
    {
        AzureAdapter m_adapter;
        public TeamProject? SelectedProject { get; set; }
        public BuildPipeline? SelectedPipeline { get; set; }
        public BuildInfo? SelectedBuild { get; set; }
        public TestRun? SelectedTestRun { get; set; }

        public event StageSwitchHandler StageSwitchEvent;

        public NavigationStage SelectedStage { get; private set; }

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

        internal void StageSwitch(int index)
        {
            StageSwitchEvent(index);
        }

        private void ResetSelectables()
        {

            SelectedProject = null;
            SelectedPipeline = null;
            SelectedBuild = null;
            SelectedTestRun = null;
            SelectedStage = NavigationStage.Connect;
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
