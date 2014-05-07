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
        private List<IScienceDataTransmitter> tranList = new List<IScienceDataTransmitter>();

        private void Start()
        {
            GameEvents.OnVesselRecoveryRequested.Add(RecoveryWatcher);
            GameEvents.onFlightReady.Add(SciScenarioStarter);
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

        private void TransmissionWatcher()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            tranList = v.FindPartModulesImplementing<IScienceDataTransmitter>();
            List<DMScienceScenario.DMScienceData> DMdataList = new List<DMScienceScenario.DMScienceData>();
            List<ScienceData> relevantDataList = new List<ScienceData>();
            foreach (IScienceDataTransmitter transmitter in tranList)
            {
                if (transmitter.IsBusy())
                {
                    List<ScienceData> dataList = new List<ScienceData>();
                    foreach (IScienceDataContainer cont in v.FindPartModulesImplementing<IScienceDataContainer>())
                    {
                        dataList.AddRange(cont.GetData());
                    }
                    if (dataList.Count > 0)
                    {
                        foreach (ScienceData data in dataList)
                        {
                            foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                            {
                                if (data.subjectID == DMData.id)
                                {
                                    relevantDataList.Add(data);
                                    DMdataList.Add(DMData);
                                }
                            }
                        }
                    }
                        
                        
                        
                        
                    
                }
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnVesselRecoveryRequested.Remove(RecoveryWatcher);
        }
    }
}
