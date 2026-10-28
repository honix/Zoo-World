using UnityEngine;

namespace ZooWorld.Game.Configs
{
    /// <summary>
    /// A label an animal carries ("Animal", "Frog", "Insect"...). Diets are lists of tags,
    /// so new food chain relations are made of assets, not code.
    /// </summary>
    [CreateAssetMenu(menuName = "Zoo World/Animal Tag", fileName = "Tag")]
    public sealed class AnimalTag : ScriptableObject
    {
        [TextArea]
        [SerializeField] private string _description;
    }
}
