using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputResource : MonoBehaviour
{
    public static PlayerInput playerInput;
    public static Dictionary<System.Guid, string> overrides;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable() { playerInput.PlayerControls.Enable(); }
    private void OnDisable() { playerInput.PlayerControls.Disable(); }

    public void SaveOverrides()
    {
        overrides = new Dictionary<System.Guid, string>();
        foreach (var map in playerInput.asset.actionMaps) // https://forum.unity.com/threads/saving-user-bindings.805722/#post-5384310
        {
            foreach (var binding in map.bindings)
            {
                if (!string.IsNullOrEmpty(binding.overridePath))
                    overrides[binding.id] = binding.overridePath;
            }
        }

        foreach (var item in overrides.Values)
        {
            print(item);
        }
    }

    public void LoadOverrides() // temp
    {
        foreach (var map in playerInput.asset.actionMaps)
        {
            var bindings = map.bindings;
            for (var i = 0; i < bindings.Count; ++i)
            {
                if (overrides.TryGetValue(bindings[i].id, out var overridePath))
                    map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
            }
        }
    }
}
