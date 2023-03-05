using SS3D.Data.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// All the relevant data for a role, including it's name, default ID Card and PDA, 
    /// Permissions and Starting Items
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "Role Data", menuName = "Roles/RoleData")]
    public class RoleData : ScriptableObject
    {
        [SerializeField] private string _roleName;
        [SerializeField] private ItemId _pdaPrefab;
        [SerializeField] private ItemId _idCardPrefab;
        [SerializeField] private List<IDPermission> _permissions = new List<IDPermission>();
        [SerializeField] private RoleLoadout _loadout;

        public string Name => _roleName;
        public ItemId IDCardPrefab => _idCardPrefab;
        public ItemId PDAPrefab => _pdaPrefab;
        public List<IDPermission> Permissions => _permissions;
        public RoleLoadout Loadout => _loadout;
    }
}
