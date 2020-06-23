using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SupportedServerConfigurations", menuName = "SupportedServerConfigurations")]
public class SupportedServerConfigurations : ScriptableObject
{
    public List<ServerConfiguration> ServerConfigurations;
}