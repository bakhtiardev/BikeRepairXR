using System.Collections.Generic;
using System.Linq;
using AUIT.AdaptationObjectives;
using AUIT.AdaptationObjectives.Definitions;
using AUIT.Extras;
using AUIT.Solvers;
using Numpy;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AUIT.AdaptationTriggers
{
    public class OnRequestOptimizationTrigger : AdaptationTrigger
    {
        // public delegate void OnUserOptimize(List<List<LocalObjective>> localObjectives, List<MultiElementObjective> m_multiElementObjectives);
        // public OnUserOptimize onUserOptimize;

        public bool debugCost;
        
        /*public async void UserApplyStrategy()
        {
            if (onUserOptimize != null)
            {
                onUserOptimize(Auit.gatherOptimizationData().objectives, Auit.MultiElementObjectives);
            }
            ApplyStrategy();
        }*/

        public override async void ApplyStrategy()
        {
            if (enabled == false)
                return;
            
            OptimizationResponse response = await Auit.OptimizeLayout();
            
            if (debugCost)
                DebugCost(response.suggested.elements.First());
            
            if (response != null)
                Auit.Adapt(response.solutions);
        }
        
        public void DebugCost(Layout layout)
        {
            print("Debugging");
            //float cost = CostFunction(layout);
            List<List<LocalObjective>> localObjectives = Auit.GetLayoutsAndLocalObjectives().Item1;
            List<MultiElementObjective> globalObjectives = Auit.MultiElementObjectives;
            List<List<float>> objectiveCosts;
            List<float> multiObjectiveCosts;
            (objectiveCosts, multiObjectiveCosts) = Utils.ComputeCostsUnweighted(new List<Layout>() { layout }, localObjectives, globalObjectives);
            string debug = "";
            for (int i = 0; i < localObjectives.Count; i++)
            {
                for (int j = 0; j < localObjectives[i].Count; j++)
                {
                    debug += localObjectives[i][j].gameObject.name + ", " + localObjectives[i][j].GetType().Name + ": " + objectiveCosts[i][j] + "\n";
                }
            }
            for (int i = 0; i < globalObjectives.Count; i++)
            {
                debug += "global, " + globalObjectives[i].GetType().Name + ": " + multiObjectiveCosts[i] + "\n";
            }
            Debug.Log(debug);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OnRequestOptimizationTrigger))]
    public class MyComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default Inspector
            DrawDefaultInspector();

            // Add a custom button
            OnRequestOptimizationTrigger trigger = (OnRequestOptimizationTrigger)target;
            if (GUILayout.Button("Request Optimization"))
            {
                trigger.ApplyStrategy();
            }
        }
    }
#endif
}