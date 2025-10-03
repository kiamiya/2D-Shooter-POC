using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class WeaponAnchors : MonoBehaviour
    {
        [SerializeField] List<Transform> anchors = new List<Transform>();
        Dictionary<Transform, bool> anchorStatus = new Dictionary<Transform, bool>();
        public List<Transform> Anchors => anchors;
        public Dictionary<Transform, bool> AnchorStatus => anchorStatus;
        private void Awake()
        {
            foreach (var anchor in anchors)
            {
                anchorStatus[anchor] = false;
            }
        }
        void SetAnchorStatus(Transform anchor, bool status)
        {
            if (anchorStatus.ContainsKey(anchor))
            {
                anchorStatus[anchor] = status;
            }
        }
        public void SetupAnchor(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[WeaponAnchors] Prefab null passé à SetupAnchor.", this);
                return;
            }

            var freeAnchor = GetFirstFreeAnchor();
            if (freeAnchor == null)
            {
                Debug.LogWarning("[WeaponAnchors] Aucun anchor libre disponible.", this);
                return;
            }

            Instantiate(prefab, freeAnchor);
            SetAnchorStatus(freeAnchor, true);
        }
        Transform GetFirstFreeAnchor()
        {
            // On suit l'ordre de la liste anchors pour garantir un ordre déterministe
            for (int i = 0; i < anchors.Count; i++)
            {
                var a = anchors[i];
                if (a == null) continue;

                if (anchorStatus.TryGetValue(a, out var used) && !used)
                {
                    return a;
                }
            }
            return null;
        }
    }
}