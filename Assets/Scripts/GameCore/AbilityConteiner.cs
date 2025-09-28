using System.Collections.Generic;
using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "AbilityConteiner", menuName = "Shashki/Abilities/AbilityConteiner", order = 2)]

    public class AbilityConteiner : ScriptableObject
    {
        [SerializeField] private List<AbilityBase> _abilities = new List<AbilityBase>();
        
        public List<AbilityBase> Abilities => _abilities;
        public AbilityBase GetRandom => _abilities[Random.Range(0, _abilities.Count)];
    }
}