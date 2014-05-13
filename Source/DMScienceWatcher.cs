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
        protected MagicDataTransmitter magicTransmitter;
        protected List<ScienceData> dataList = new List<ScienceData>();

        private void Start()
        {
            GameEvents.OnVesselRecoveryRequested.Add(RecoveryWatcher);
            GameEvents.onVesselRecovered.Add(ProtoRecoveryWatcher);
            GameEvents.onFlightReady.Add(SciScenarioStarter);


            //When the vessel is actually ready to control, after onInitialize?
            GameEvents.onFlightReady.Add(flightready);
            //When the vessel is first loaded
            GameEvents.onVesselLoaded.Add(vesselload);
            //When vessel is changed, applies on load
            GameEvents.onVesselChange.Add(vesselchange);
            //triggered when recovered from in-flight
            GameEvents.OnVesselRecoveryRequested.Add(recoveryrequest);
            //When vessel is recovered from tracking center/spacecenter, also triggered after recovery requested
            GameEvents.onVesselRecovered.Add(vesselrecovered);
        }


        private void OnDestroy()
        {
            GameEvents.OnVesselRecoveryRequested.Remove(RecoveryWatcher);
            GameEvents.onFlightReady.Remove(SciScenarioStarter);
        }

        private void SciScenarioStarter()
        {
            //DMScienceScenario Scenario = DMScienceScenario.SciScenario;
        }

        private void vesselrecovered(ProtoVessel v)
        {
            EventDebug("vessel recover");
        }

        private void recoveryrequest(Vessel v)
        {
            EventDebug("recovery request");
        }

        private void vesselchange(Vessel v)
        {
            EventDebug("vessel change");
        }

        private void vesselload(Vessel v)
        {
            EventDebug("vessel load");
        }

        private void flightready()
        {
            EventDebug("flight ready");
        }

        private void EventDebug(string gevent)
        {
            print("GameEvent " + gevent + " triggered");
        }

        private void RecoveryWatcher(Vessel v)
        {
            dataList.Clear();
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

        private void ProtoRecoveryWatcher(ProtoVessel v)
        {
            EventDebug(v.vesselName);
            foreach (ProtoPartSnapshot snap in v.protoPartSnapshots)
            {
                foreach (ProtoPartModuleSnapshot msnap in snap.modules)
                {
                    foreach (ConfigNode dataNode in msnap.moduleValues.GetNodes("ScienceData"))
                    {
                        string id = dataNode.GetValue("subjectID");
                        print("Found science data with subject ID: " + id);
                        ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(id);
                        if (sub != null)
                        {
                            foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                            {
                                if (DMData.id == sub.id)
                                {
                                    DMModuleScienceAnimate.submitDMScience(DMData, sub);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<ScienceData> TransmissionList(Vessel v)
        {
            dataList.Clear();
            magicTransmitter = v.FindPartModulesImplementing<MagicDataTransmitter>().First();
            if (magicTransmitter != null)
            {
                dataList = magicTransmitter.QueuedData;
            }
            return dataList;
        }

        private void TransmissionWatcher(Vessel v)
        {
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
