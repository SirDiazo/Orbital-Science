using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMagic
{
    [KSPAddonImproved(KSPAddonImproved.Startup.Flight, false)]
    class DMTransmissionWatcher: MonoBehaviour
    {
        private DMMagicManager manager;
        internal static bool EvilReeper;
        private bool listCleared = false;
        private List<ScienceData> lastData = new List<ScienceData>();

        private void Start()
        {
            print("[DM] Starting Transmission Watcher");
            GameEvents.onFlightReady.Add(MagicTransmitter);
            ScienceAlert();
        }

        private void OnDestroy()
        {
            print("[DM] Stopping Transmission Watcher");
            GameEvents.onFlightReady.Remove(MagicTransmitter);
        }

        private void Update()
        {
            if (FlightGlobals.ready)
            {
                if (TransmissionList().Count > 0)
                {
                    listCleared = false;
                    foreach (ScienceData data in TransmissionList())
                    {
                        TransmissionData(data);
                    }
                }
                else if (!listCleared)
                {
                    lastData.Clear();
                    listCleared = true;
                }
            }
        }

        private void ScienceAlert()
        {
            print("[DM] Searching for Science Alert...");
            var SciAlert = AssemblyLoader.loadedAssemblies.SingleOrDefault(a => a.assembly.GetName().Name == "ScienceAlert");
            if (SciAlert != null)
            {
                print("Found Science Alert");
                EvilReeper = true;
            }
            else
            {
                print("[DM] Science Alert Not Found");
                EvilReeper = false;
            }
        }

        private void MagicTransmitter()
        {
            if (FlightGlobals.ready)
            {
                if (manager == null)
                {
                    print("[DM] Adding manager");
                    manager = gameObject.AddComponent<DMMagicManager>();
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

        private void TransmissionData(ScienceData oldData)
        {
            if (!lastData.Contains(oldData))
            {
                TransmitData(oldData);
                lastData.Add(oldData);
            }
        }

        private void TransmitData(ScienceData data)
        {
            print("[DM] Found Transmitting Data");
            foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
            {
                if (data.title == DMData.title)
                {
                    float subVal = DMScienceScenario.SciScenario.SciSub(data.subjectID);
                    ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
                    sub.scientificValue = DMData.scival;
                    DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                    DMScienceScenario.SciScenario.updateRemainingData();
                }
            }
        }
    }
}
