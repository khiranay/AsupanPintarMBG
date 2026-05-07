using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════
//  BackButtonHandler
//  Attach ke tombol Back di scene RouteMap atau halaman manapun
//  yang ingin langsung kembali ke RouteMap.
// ═══════════════════════════════════════════════════════════════
public class BackButtonHandler : MonoBehaviour
{
    public void OnBackToRouteMap()
    {
        LevelFlowManager.GoToRouteMap();
    }
}


