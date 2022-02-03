using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using Utility;

namespace Managers
{
    public class PlayerManager : MonoSingleton<PlayerManager>
    {
        private readonly Dictionary<int, Color> _playerTeamTeamColours = new();

        private void OnEnable()
        {
            GenerateTeamColours(new List<Color>{Color.green,Color.red,Color.blue,Color.cyan,Color.gray,Color.magenta});
        }

        private void GenerateTeamColours([NotNull] List<Color> colors)
        {
            if (colors == null) throw new ArgumentNullException(nameof(colors));
            for (var i = 0; i < colors.Count; i++)
            {
                _playerTeamTeamColours.Add(i,colors[i]);
            }
        }

        public Color GetColourFromTeamID(int teamID)
        {
            return teamID >= _playerTeamTeamColours.Count ? _playerTeamTeamColours[0] : _playerTeamTeamColours[teamID];
        }
    }
}
