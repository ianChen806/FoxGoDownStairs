using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FloorManager : MonoBehaviour
{
    [FormerlySerializedAs("FloorPrefab")]
    [SerializeField]
    private GameObject[] floorPrefab;

    public void CreateFloor()
    {
        var prefab = floorPrefab
            .Select(o => new { Order = Guid.NewGuid(), Self = o })
            .OrderBy(r => r.Order)
            .First()
            .Self;
        var floor = Instantiate(prefab, transform);

        var x = Random.Range(-6.52f, 3.51f);
        floor.transform.position = new Vector3(x, -5, 0);
    }
}
