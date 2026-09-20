# VR Central Hub & Development Suite (`com.vrcore.hub`)

A modular, cross-version Unity Editor framework designed as the single source of truth for developing VR games, simulations, and spatial computing applications.

---

## 🚀 Key Modules Included

1. **📦 VR Package & Dependency Manager**
   - Automated inspection and 1-click installation of OpenXR, XR Interaction Toolkit (XRI), XR Hands, and Meta Quest SDKs.
2. **⚙️ Universal Project Settings Configurator**
   - Cross-version validation and 1-click automated configuration for Player Settings (Linear Color Space, IL2CPP, ARM64), XR Loaders, Input System, ASTC compression, MSAA 4x, and VR shadow distance.
3. **📁 Project Folder Scaffolder**
   - 1-click generation of industry-standard VR project directory structures with `.gitkeep` files and `PROJECT_STRUCTURE.md` guides.
4. **📊 VR Performance Profiler & Optimizer**
   - Audits project meshes, textures, mipmaps, audio RAM usage, realtime lighting, and physics fixed timestep synchronization against Meta Quest and PCVR frame budgets (72Hz / 90Hz / 120Hz).
5. **🥽 Dual-Stack (XRI & Meta XR) Scene & Interaction Builder**
   - Live development authoring palette to spawn production XR Origin Rigs, grabbable props with attach points, spring push buttons, rotating levers, hinged doors, snap sockets, body holsters, and world-space VR UI.

---

## 📥 How to Install in Any Unity Project via Git

### Method 1: Install from your Existing Git Repository (Direct Subfolder)
In any Unity project:
1. Open **Window** > **Package Manager**.
2. Click the **`+`** (Add) button in the top-left corner.
3. Select **"Add package from git URL..."**.
4. Enter your repository URL with the `?path=` parameter:
   ```
   https://github.com/<YourUsername>/<YourRepoName>.git?path=/Assets/VRCore
   ```
5. Click **Add**. Unity will pull and install only the VR Central Hub system as an immutable package!

---

### Method 2: Install via `Packages/manifest.json`
Add the following line to your target project's `Packages/manifest.json` under `"dependencies"`:
```json
"com.vrcore.hub": "https://github.com/<YourUsername>/<YourRepoName>.git?path=/Assets/VRCore"
```

---

## 🛠️ Usage
Once installed, open Unity's top menu and navigate to:
```
VR Core > VR Central Hub
```
