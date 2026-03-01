using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Repair/Repair Option")]
public class RepairOption : ScriptableObject
{
    public string repairName;

    [TextArea(3, 10)]
    public List<string> steps = new List<string>();
}