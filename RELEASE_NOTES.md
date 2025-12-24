# WingetUpdater Release Notes

## Version 1.0.0 (2024-12-23)

### 🎉 Initial Release

Primera versión pública de WingetUpdater - Gestor moderno de software para Windows.

### ✨ Características Principales

#### Gestión de Software
- ✅ **Lista completa de programas** instalados en el sistema
- ⚡ **Actualización individual** con botón en cada programa
- 🗑️ **Desinstalación limpia** con eliminación automática de:
  - Archivos residuales en `%APPDATA%`
  - Archivos residuales en `%LOCALAPPDATA%`
  - Accesos directos huérfanos
  - **NO toca el registro** (siguiendo recomendaciones de Microsoft)
- 📊 **Logs en tiempo real** de todas las operaciones

#### Interfaz Moderna
- 🎨 Diseño basado en **Fluent Design System 2024-2025**
- 🔵 **Microsoft Blue** como color principal (transmite confianza)
- ♿ **WCAG AA compliant** - Contraste mínimo 4.5:1
- 📱 Interfaz responsive y optimizada
- 🌓 Panel de logs colapsable para UX limpia

#### Servicios Backend
- **WingetService** - Integración completa con Windows Package Manager
- **UninstallService** - Desinstalación limpia con búsqueda de residuos
- **CleanupService** - Limpieza de archivos temporales y caché (preparado para futuras versiones)
- **AdminHelper** - Detección de privilegios de administrador

### 🔧 Tecnologías

- .NET 8.0
- WPF (Windows Presentation Foundation)
- MVVM Architecture
- Windows Package Manager (winget)
- Official Windows APIs (SHEmptyRecycleBin, Path.GetTempPath)

### 📦 Instalación

**Requisitos:**
- Windows 10/11 (versión 1809 o superior)
- .NET 8.0 Runtime
- Windows Package Manager (winget)

**Ejecutable Precompilado:**
1. Descarga `WingetUpdater-v1.0.0-win-x64.zip`
2. Extrae el contenido
3. Ejecuta `WingetUpdater.exe`

### 🐛 Problemas Conocidos

- Windows Defender puede mostrar advertencia al ejecutar (normal para apps sin certificado firmado)
- Algunos programas no se pueden desinstalar si no están en el índice de winget

### 🔮 Próximas Características (v1.1.0)

- [ ] Sistema de pestañas (Actualizaciones | Programas | Limpieza)
- [ ] Interfaz gráfica para limpieza del sistema
- [ ] Soporte multiidioma
- [ ] Programación de actualizaciones automáticas
- [ ] Historial de actualizaciones/desinstalaciones

### 👨‍💻 Contribuciones

¡Las contribuciones son bienvenidas! Ver [CONTRIBUTING.md](CONTRIBUTING.md) para más detalles.

### 📝 Licencia

MIT License - Ver [LICENSE](LICENSE) para detalles completos.

---

**Repositorio:** https://github.com/CharlieCardenasToledo/WPF-App
**Autor:** Charlie Cárdenas Toledo
