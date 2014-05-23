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
            GameEvents.onVesselRecovered.Remove(ProtoRecoveryWatcher);
            GameEvents.onFlightReady.Remove(SciScenarioStarter);
        }

        private void Update()
        {
            if (FlightGlobals.ready && activeVessel.loaded)
            {
                if (TransmissionList().Count > 0)
                {
                    TransmissionWatcher();
                }
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
            var managerObj = new GameObject();
            managerObj.AddComponent<MagicManager>();
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

        private void ProtoRecoveryWatcher(ProtoVessel v)
        {
            EventDebug(v.vesselName);
            foreach (ProtoPartSnapshot snap in v.protoPartSnapshots)
            {
                foreach (ProtoPartModuleSnapshot msnap in snap.modules)
                {
                    //foreach (ScienceData science in data.
                    foreach (ConfigNode dataNode in msnap.moduleValues.GetNodes("ScienceData"))
                    {
                        ScienceData data = new ScienceData(dataNode);
                            foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                            {
                                if (DMData.title == data.title)
                                {
                                    float subVal = SciSub (data.subjectID);
                                    data.dataAmount = subVal * DMData.basevalue * DMData.scival * DMData.dataScale;
                                    DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                                }
                            }
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
                        if (data.title == DMData.title)
                        {
                            float subVal = SciSub(data.subjectID);
                            data.dataAmount = subVal * DMData.basevalue * DMData.scival * DMData.dataScale;
                            DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                            updateRemainingData();
                        }
                    }
                }
            }
        }

        private void updateRemainingData ()
        {
            List<ScienceData> dataList = new List<ScienceData>();
            foreach (IScienceDataContainer container in FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataContainer>())
            {
                dataList.AddRange(container.GetData());
            }
            foreach (ScienceData data in dataList)
            {
                foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                {
                    if (DMData.title == data.title)
                    {
                        float subV = SciSub(data.subjectID);
                        data.dataAmount = DMData.basevalue * DMData.dataScale * DMData.scival * subV;
                    }
                }
            }
        }

        private float SciSub (string s)
        {
            switch (s[s.Length - 1])
            {
                case 'A':
                    return 1.5f;
                case 'B':
                    return 3f;
                case 'C':
                    return 5f;
                case 'D':
                    return 8f;
                case 'E':
                    return 10f;
                default:
                    return 30f;
            }
        }

    }
}
