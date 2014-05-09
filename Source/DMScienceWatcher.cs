using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMagic
{
    [KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(DMScienceWatcher))]
    internal class DMScienceWatcher: MonoBehaviour
    {

        private void Start()
        {
            GameEvents.OnVesselRecoveryRequested.Add(RecoveryWatcher);
            GameEvents.onFlightReady.Add(SciScenarioStarter);
        }

        private void OnDestroy()
        {
            GameEvents.OnVesselRecoveryRequested.Remove(RecoveryWatcher);
            GameEvents.onFlightReady.Remove(SciScenarioStarter);
        }

        private void SciScenarioStarter()
        {
            DMScienceScenario Scenario = DMScienceScenario.SciScenario;
        }

        private void RecoveryWatcher(Vessel v)
        {
            List<ScienceData> dataList = new List<ScienceData>();
            foreach (IScienceDataContainer cont in v.FindPartModulesImplementing<IScienceDataContainer>())
            {
                dataList.AddRange(cont.GetData());
            }
            foreach (ScienceData data in dataList)
            {
                foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                {
                    if (data.subjectID == DMData.id)
                    {
                        ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
                        DMModuleScienceAnimate.submitDMScience(DMData, sub);
                    }
                }
            }
        }

    }
}
