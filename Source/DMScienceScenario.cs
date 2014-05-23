/* DMagic Orbital Science - DM Science Scenario
 * Scenario Module to store science results
 *
 * Copyright (c) 2014, David Grandy <david.grandy@gmail.com>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 *  
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used 
 * to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *  
 */


using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace DMagic
{
    public class DMScienceScenario: ScenarioModule
    {

        public static DMScienceScenario SciScenario
        {
            get
            {
                Game g = HighLogic.CurrentGame;
                foreach (ProtoScenarioModule pMod in g.scenarios)
                {
                    if (pMod.moduleName == typeof(DMScienceScenario).Name) return (DMScienceScenario)pMod.moduleRef;
                }
                return (DMScienceScenario)g.AddProtoScenarioModule(typeof(DMScienceScenario), GameScenes.FLIGHT | GameScenes.TRACKSTATION | GameScenes.SPACECENTER).moduleRef;
            }
            set {}
        }

        internal static List<DMScienceData> recoveredScienceList = new List<DMScienceData>();

        public override void OnSave(ConfigNode node)
        {
            ConfigNode results_node = new ConfigNode("Asteroid Science");
            foreach (DMScienceData data in recoveredScienceList)
            {
                ConfigNode scienceResults_node = new ConfigNode("Science");
                scienceResults_node.AddValue("id", data.id);
                scienceResults_node.AddValue("title", data.title);
                scienceResults_node.AddValue("bsv", data.basevalue);
                scienceResults_node.AddValue("dsc", data.dataScale);
                scienceResults_node.AddValue("scv", data.scival);
                //scienceResults_node.AddValue("sbv", data.subval);
                scienceResults_node.AddValue("sci", data.science);
                scienceResults_node.AddValue("cap", data.cap);
                scienceResults_node.AddValue("expNo", data.expNo);
                results_node.AddNode(scienceResults_node);
            }
            node.AddNode(results_node);
        }

        public override void OnLoad(ConfigNode node)
        {
            recoveredScienceList.Clear();
            ConfigNode results_node = node.GetNode("Asteroid Science");
            if (results_node != null)
            {
                foreach (ConfigNode scienceResults_node in results_node.GetNodes("Science"))
                {
                    string id = scienceResults_node.GetValue("id");
                    string title = scienceResults_node.GetValue("title");
                    float bsv = Convert.ToSingle(scienceResults_node.GetValue("bsv"));
                    float dsc = Convert.ToSingle(scienceResults_node.GetValue("dsc"));
                    float scv = Convert.ToSingle(scienceResults_node.GetValue("scv"));
                    //float sbv = Convert.ToSingle(scienceResults_node.GetValue("sbv"));
                    float sci = Convert.ToSingle(scienceResults_node.GetValue("sci"));
                    float cap = Convert.ToSingle(scienceResults_node.GetValue("cap"));
                    int eNo = Convert.ToInt32(scienceResults_node.GetValue("expNo"));
                    RecordNewScience(id, title, bsv, dsc, scv, sci, cap, eNo);
                }
            }
        }
        
        internal class DMScienceData
        {
            internal string id, title;
            internal int expNo;
            internal float dataScale, scival, science, cap, basevalue;
        }

        internal void RecordNewScience(string id, string title, float baseval, float dsc, float scv, float sci, float cap, int eNo)
        {
            DMScienceData DMData = new DMScienceData();
            DMData.id = id;
            DMData.title = title;
            DMData.basevalue = baseval;
            DMData.dataScale = dsc;
            DMData.scival = scv;
            //DMData.subval = sbv;
            DMData.science = sci;
            DMData.cap = cap;
            DMData.expNo = eNo;
            recoveredScienceList.Add(DMData);
            print("Adding new DMData to list");
            //return DMData;
        }

        internal void UpdateNewScience(DMScienceData DMData)
        {
            foreach (DMScienceData DMSci in recoveredScienceList)
            {
                if (DMSci.title == DMData.title)
                {
                    DMSci.science = DMData.science;
                    DMSci.expNo = DMData.expNo;
                    DMSci.scival = DMData.scival;
                }
            }
        }

        internal void submitDMScience(DMScienceData DMData, float subVal)
        {
            DMData.scival = ScienceValue(DMData.expNo, DMData.scival);
            DMData.science += DMData.basevalue * subVal * DMData.scival;
            DMData.expNo++;
            UpdateNewScience(DMData);
        }

        internal void RemoveDMScience(DMScienceData DMdata)
        {
            recoveredScienceList.Remove(DMdata);
        }

        internal float ScienceValue (int i, float f)
        {
            float sciVal = 1f;
            if (i < 3) sciVal = f - 0.05f * (6 / i);
            else sciVal = f - 0.05f;
            return sciVal;
        }

    }
}
