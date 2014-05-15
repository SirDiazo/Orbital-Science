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
        protected Vessel activeVessel;
        protected MagicManager manager;

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

        private void Update()
        {
            if (FlightGlobals.ready && activeVessel.loaded)
            {
                TransmissionWatcher();
            }
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
            activeVessel = FlightGlobals.ActiveVessel;
            manager = null;
            manager = gameObject.AddComponent<MagicManager>();
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
            //dataList.Clear();
            //foreach (IScienceDataContainer cont in v.FindPartModulesImplementing<IScienceDataContainer>())
            //{
            //    dataList.AddRange(cont.GetData());
            //}
            //foreach (ScienceData data in dataList)
            //{
            //    foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
            //    {
            //        if (data.subjectID == DMData.id)
            //        {
            //            ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
            //            DMModuleScienceAnimate.submitDMScience(DMData, sub);
            //        }
            //    }
            //}
        }

        private void ProtoRecoveryWatcher(ProtoVessel v)
        {
            ////ConfigNode node = new ConfigNode();
            ////ScienceData data = new ScienceData(node);
            EventDebug(v.vesselName);
            foreach (ProtoPartSnapshot snap in v.protoPartSnapshots)
            {
                foreach (ProtoPartModuleSnapshot msnap in snap.modules)
                {
                    //foreach (ScienceData science in data.
                    foreach (ConfigNode dataNode in msnap.moduleValues.GetNodes("ScienceData"))
                    {
                        ScienceData data = new ScienceData(dataNode);
                        //string id = dataNode.GetValue("subjectID");
                        //print("Found science data with subject ID: " + id);
                        //if (!string.IsNullOrEmpty(id))
                        //{
                            foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                            {
                                if (DMData.id == data.subjectID)
                                {
                                    data.dataAmount *= DMData.scival;
                                    float subVal = data.dataAmount / (DMData.dataScale * DMData.basevalue);
                                    DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                                }
                            }
                        //}
                    }
                }
            }
        }

        private List<ScienceData> TransmissionList()
        {
            List<ScienceData> dataList = new List<ScienceData>();
            if (manager != null)
                dataList = manager.MagicScienceList();
            return dataList;
        }

        private void TransmissionWatcher()
        {
            List<ScienceData> MTdataList = TransmissionList();
            if (MTdataList.Count > 0)
            {
                foreach (ScienceData data in MTdataList)
                {
                    foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                    {
                        if (data.subjectID == DMData.id)
                        {
                            data.dataAmount *= DMData.scival;
                            float subVal = data.dataAmount / (DMData.dataScale * DMData.basevalue);
                            DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                        }
                    }
                }
            }
        }

    }
}
