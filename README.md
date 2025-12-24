# WingetUpdater - Modern Software Manager for Windows

<div align="center">

![WingetUpdater](https://img.shields.io/badge/Platform-Windows-blue?logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)
![License](https://img.shields.io/badge/License-MIT-green)
![WPF](https://img.shields.io/badge/UI-WPF-00599C?logo=microsoft)

**Una aplicación moderna de WPF para gestionar actualizaciones y desinstalaciones de software en Windows usando winget**

[Características](#✨-características) • [Instalación](#🚀-instalación) • [Uso](#📖-uso) • [Tecnologías](#⚙️-tecnologías) • [Contribuir](#🤝-contribuir)

</div>

---

## 🌟 Descripción

WingetUpdater es un gestor de software moderno y elegante para Windows que aprovecha el poder de **Windows Package Manager (winget)** para:

- ✅ Listar **todos** los programas instalados en tu sistema
- 🔄 Actualizar aplicaciones con un solo clic
- 🗑️ Desinstalar programas con **limpieza profunda** de archivos residuales
- 🧹 Limpiar archivos temporales, caché y basura del sistema
- 📊 Monitoreo en tiempo real con logs detallados

Todo esto con una **interfaz moderna** basada en **Fluent Design 2024-2025** y principios de **accesibilidad WCAG AA**.

---

## ✨ Características

### 🎯 Gestión de Software
- **Lista completa de programas** instalados (no solo actualizaciones)
- **Actualización individual** con botón ⚡ en cada programa
- **Desinstalación limpia** con botón 🗑️ que elimina:
  - Programas vía winget
  - Archivos residuales en `%APPDATA%`
  - Archivos residuales en `%LOCALAPPDATA%`
  - Accesos directos huérfanos
  - **NO toca el registro** (siguiendo recomendaciones de Microsoft)

### 🧹 Limpieza del Sistema
- Archivos temporales del usuario
- Archivos temporales del sistema (requiere admin)
- Vaciado de papelera de reciclaje
- Caché de navegadores (Chrome, Edge, Firefox)
- Caché de Windows (Thumbnails, Prefetch)

### 🎨 Diseño Moderno
- **Microsoft Blue (#0078D4)** como color principal (transmite confianza)
- **Fluent Design System** con sombras sutiles para profundidad
- **Tipografía optimizada** con Segoe UI Variable
- **Contraste WCAG AA** en todas las combinaciones de colores
- **Espaciado optimizado** para reducir carga cognitiva

### 🔒 Seguridad
- Confirmaciones antes de operaciones destructivas
- Vista previa de lo que se eliminará
- Detección automática de privilegios de administrador
- Logging completo de todas las operaciones
- Basado en **documentación oficial de Microsoft**

---

## 🚀 Instalación

### Requisitos Previos

- **Windows 10/11** (versión 1809 o superior)
- **.NET 8.0 Runtime** ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Windows Package Manager (winget)** - Viene preinstalado en Windows 11, o instala [App Installer](https://www.microsoft.com/p/app-installer/9nblggh4nns1) desde Microsoft Store

### Desde el Código Fuente

1. **Clona el repositorio**
   ```bash
   git clone https://github.com/CharlieCardenasToledo/WPF-App.git
   cd WPF-App/WingetUpdater
   ```

2. **Compila el proyecto**
   ```bash
   dotnet build
   ```

3. **Ejecuta la aplicación**
   ```bash
   dotnet run
   ```

### Ejecutable Compilado

**📦 Descarga directa:** [WingetUpdater v1.0.0](https://github.com/CharlieCardenasToledo/WPF-App/releases)

1. **Descarga** el archivo `WingetUpdater-v1.0.0-win-x64.zip` desde [Releases](https://github.com/CharlieCardenasToledo/WPF-App/releases)
2. **Extrae** el contenido del ZIP a una carpeta
3. **Ejecuta** `WingetUpdater.exe`

> ⚠️ **Requisito:** Necesitas tener instalado [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) en tu sistema.

> 💡 **Nota:** Windows Defender puede mostrar una advertencia la primera vez. Esto es normal para aplicaciones sin certificado de firma. Haz clic en "Más información" → "Ejecutar de todas formas".

---

## 📖 Uso

### Interfaz Principal

1. **Lista de Programas**
   - La aplicación carga automáticamente todos los programas instalados
   - Cada programa muestra: Nombre, ID, Versión Actual, Nueva Versión (si hay), Estado

2. **Acciones Individuales**
   - **⚡ Actualizar** - Clic en el botón azul para actualizar ese programa específico
   - **🗑️ Desinstalar** - Clic en el botón rojo para desinstalar con limpieza profunda

3. **Acciones Masivas** (botones superiores)
   - **Seleccionar Todo** / **Deseleccionar Todo** - Para operaciones en lote
   - **Actualizar Seleccionados** - Actualiza múltiples programas a la vez
   - **Actualizar Todo** - Actualiza todos los programas con actualizaciones
   - **Desinstalar Seleccionados** - Desinstala múltiples programas

4. **Logs Técnicos**
   - Botón "Mostrar Detalles Técnicos" para ver logs en tiempo real
   - Panel colapsable para mantener la UI limpia

### Desinstalación Limpia

Cuando desinstalas un programa, WingetUpdater:
1. Ejecuta `winget uninstall --id "PackageId" --exact --silent`
2. Busca archivos residuales en:
   - `%APPDATA%\[ProgramName]`
   - `%LOCALAPPDATA%\[ProgramName]`
   - Accesos directos en Desktop y Start Menu
3. Elimina los archivos encontrados (con tu confirmación)
4. Muestra un resumen de lo eliminado

---

## ⚙️ Tecnologías

### Frontend
- **WPF (Windows Presentation Foundation)** - Framework de UI
- **XAML** - Markup para diseño de interfaz
- **Fluent Design System** - Principios de diseño de Microsoft

### Backend
- **.NET 8.0** - Framework de desarrollo
- **C#** - Lenguaje de programación
- **MVVM Pattern** - Arquitectura (Model-View-ViewModel)

### APIs y Servicios
- **Windows Package Manager (winget)** - Gestión de paquetes
- **SHEmptyRecycleBin** (P/Invoke) - API oficial para vaciar papelera
- **Path.GetTempPath()** - API de .NET para archivos temporales
- **Environment.SpecialFolder** - Rutas seguras del sistema

### Integración
- `System.Diagnostics.Process` - Ejecución de comandos winget
- `System.IO` - Gestión de archivos y directorios
- `System.Runtime.InteropServices` - P/Invoke para APIs de Windows

---

## 🏗️ Arquitectura

```
WingetUpdater/
├── Models/              # Modelos de datos (PackageInfo, etc.)
├── ViewModels/          # Lógica de presentación (MVVM)
├── Views/              # Interfaces de usuario (XAML)
├── Services/
│   ├── WingetService.cs       # Integración con winget
│   ├── CleanupService.cs      # Limpieza del sistema
│   └── UninstallService.cs    # Desinstalación limpia
├── Helpers/
│   └── AdminHelper.cs         # Detección de privilegios
├── Commands/           # RelayCommand para MVVM
├── Converters/         # Value converters para XAML
└── Resources/
    └── Styles.xaml            # Estilos modernos Fluent Design
```

---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Si quieres mejorar WingetUpdater:

1. **Fork** el proyecto
2. Crea una **rama** para tu feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** tus cambios (`git commit -m 'Add: Amazing Feature'`)
4. **Push** a la rama (`git push origin feature/AmazingFeature`)
5. Abre un **Pull Request**

### Áreas de Mejora

- [ ] Sistema de pestañas completo (Actualizaciones | Programas | Limpieza)
- [ ] Interfaz para limpieza de sistema desde la UI
- [ ] Soporte para múltiples idiomas
- [ ] Empaquetado como MSIX para Microsoft Store
- [ ] Programación de actualizaciones automáticas
- [ ] Historial de actualizaciones y desinstalaciones
- [ ] Restaurar programas desinstalados

---

## 📝 Investigación y Mejores Prácticas

Este proyecto fue desarrollado siguiendo **documentación oficial de Microsoft** y mejores prácticas de la industria:

- ✅ **Comandos winget oficiales** (`--exact`, `--silent`, `--source winget`)
- ✅ **APIs oficiales de Windows** (`SHEmptyRecycleBin`, `Path.GetTempPath()`)
- ✅ **Environment.SpecialFolder** para rutas seguras
- ⚠️ **NO limpieza automática del registro** (riesgoso según Microsoft)
- ✅ **WCAG AA compliance** para accesibilidad
- ✅ **Fluent Design principles** para UI moderna

---

## 📜 Licencia

Este proyecto está bajo la Licencia MIT. Ver archivo `LICENSE` para más detalles.

---

## 👨‍💻 Autor

**Charlie Cárdenas Toledo**

- GitHub: [@CharlieCardenasToledo](https://github.com/CharlieCardenasToledo)

---

## 🙏 Agradecimientos

- **Microsoft** por Windows Package Manager (winget)
- **Fluent Design System** por los principios de diseño
- La comunidad de **.NET** y **WPF** por recursos y documentación

---

<div align="center">

⭐ **Si te gusta este proyecto, dale una estrella!** ⭐

</div>
