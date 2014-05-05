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

        private List<DMScienceData> recoveredScienceList = new List<DMScienceData>();

        public override void OnSave(ConfigNode node)
        {
            ConfigNode results_node = new ConfigNode("Asteroid Science");
            results_node.ClearNodes();
            foreach (DMScienceData data in recoveredScienceList)
            {
                ConfigNode scienceResults_node = new ConfigNode("Science");
                scienceResults_node.AddValue("id", data.id);
                scienceResults_node.AddValue("title", data.title);
                scienceResults_node.AddValue("dsc", data.dsc);
                scienceResults_node.AddValue("scv", data.scv);
                scienceResults_node.AddValue("sbv", data.sbv);
                scienceResults_node.AddValue("sci", data.sci);
                scienceResults_node.AddValue("cap", data.cap);
            }
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
                    double dsc = Convert.ToDouble(scienceResults_node.GetValue("dsc"));
                    double scv = Convert.ToDouble(scienceResults_node.GetValue("scv"));
                    double sbv = Convert.ToDouble(scienceResults_node.GetValue("sbv"));
                    double sci = Convert.ToDouble(scienceResults_node.GetValue("sci"));
                    double cap = Convert.ToDouble(scienceResults_node.GetValue("cap"));
                    RecordNewScience(id, title, dsc, scv, sbv, sci, cap);
                }
            }
        }
        
        internal class DMScienceData
        {
            internal string id, title;
            internal double dsc, scv, sbv, sci, cap;
        }

        internal void RecordNewScience(string id, string title, double dsc, double scv, double sbv, double sci, double cap)
        {
            DMScienceData DMData = new DMScienceData();
            DMData.id = id;
            DMData.title = title;
            DMData.dsc = dsc;
            DMData.scv = scv;
            DMData.sbv = sbv;
            DMData.sci = sci;
            DMData.cap = cap;
            recoveredScienceList.Add(DMData);
        }

        internal void RemoveDMScience(DMScienceData DMdata)
        {
            recoveredScienceList.Remove(DMdata);
        }

    }
}
