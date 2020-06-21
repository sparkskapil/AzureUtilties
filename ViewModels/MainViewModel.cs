using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Helper;

namespace ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private int m_selectedNavItemIndex;

        public int SelectedNavItemIndex
        {
            get { return m_selectedNavItemIndex; }
            set
            {
                m_selectedNavItemIndex = value;
                RaiseEvent();
            }
        }

        public MainViewModel()
        {
            SelectedNavItemIndex = (int)SessionManager.Instance.SelectedStage;
            SessionManager.Instance.StageSwitchEvent += new StageSwitchHandler(SwitchStage);
        }

        private void SwitchStage(int index)
        {
            SelectedNavItemIndex = index;
        }

    }
}
