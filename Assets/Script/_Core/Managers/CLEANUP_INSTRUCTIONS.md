# 🧹 CLEANUP: Hapus GameObject Sisa dari Attempt Sebelumnya

Sebelum play, **HAPUS** GameObject berikut di Hierarchy Unity:

1. Buka scene apapun (Home, RouteMap, atau Materi)
2. Di Hierarchy, cari dan **HAPUS** (klik kanan → Delete):
   - `[SceneLoader]` (atau `[Auto]SceneLoader`)
   - `LoadingOverlay`
   - `__BlankingOverlay_1` s/d `__BlankingOverlay_7`
3. Di Inspector, kalau ada SceneLoader di GameObject manapun di scene, **HAPUS** script SceneLoader dari GameObject itu (klik kanan → Remove Component)
4. **Save scene** (Ctrl+S)
5. Lakukan hal yang sama untuk SEMUA scene (Home, RouteMap, Materi, Game, Kuis)

Setelah bersih, klik **Play** — script akan auto-create `[SceneLoader]` yang baru.
