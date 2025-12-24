# 🤝 Guía de Contribución

¡Gracias por tu interés en contribuir a **WingetUpdater**! 🎉

Este proyecto es de código abierto y las contribuciones de la comunidad son más que bienvenidas. Ya sea que corrijas un bug, agregues una nueva característica, mejores la documentación o simplemente reportes un problema, ¡tu ayuda es invaluable!

---

## 📋 Tabla de Contenidos

- [Código de Conducta](#-código-de-conducta)
- [¿Cómo puedo contribuir?](#-cómo-puedo-contribuir)
- [Reportar Bugs](#-reportar-bugs)
- [Proponer Nuevas Características](#-proponer-nuevas-características)
- [Pull Requests](#-pull-requests)
- [Guías de Estilo](#-guías-de-estilo)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Proceso de Desarrollo](#-proceso-de-desarrollo)

---

## 📜 Código de Conducta

Este proyecto se adhiere a un código de conducta para crear un ambiente acogedor e inclusivo:

- **Sé respetuoso** - Trata a todos con respeto y consideración
- **Sé constructivo** - Las críticas deben ser constructivas y enfocadas en el código, no en las personas
- **Sé paciente** - Todos estamos aprendiendo
- **Sé profesional** - Mantén un lenguaje profesional y apropiado

Cualquier violación de estas normas puede resultar en la prohibición de contribuir al proyecto.

---

## 🚀 ¿Cómo puedo contribuir?

Hay muchas formas de contribuir a WingetUpdater:

### 1. 🐛 Reportar Bugs
Encuentra un bug? [Abre un issue](#-reportar-bugs) con los detalles.

### 2. 💡 Proponer Características
¿Tienes una idea genial? [Propón una nueva característica](#-proponer-nuevas-características).

### 3. 📝 Mejorar Documentación
- Corregir typos
- Mejorar explicaciones
- Agregar ejemplos
- Traducir a otros idiomas

### 4. 💻 Contribuir Código
- Implementar nuevas características
- Corregir bugs
- Mejorar el rendimiento
- Refactorizar código

### 5. 🧪 Testing
- Probar en diferentes configuraciones de Windows
- Reportar problemas de compatibilidad
- Agregar tests automatizados

---

## 🐛 Reportar Bugs

### Antes de Reportar
1. **Busca en issues existentes** - Tu bug puede haber sido reportado ya
2. **Verifica tu versión** - Asegúrate de usar la última versión
3. **Reproduce el bug** - Confirma que el problema es consistente

### Cómo Reportar un Bug

Crea un issue con esta información:

```markdown
## Descripción del Bug
[Descripción clara y concisa del problema]

## Pasos para Reproducir
1. Abre la aplicación
2. Haz clic en '...'
3. Observa el error

## Comportamiento Esperado
[Qué esperabas que pasara]

## Comportamiento Actual
[Qué pasó realmente]

## Screenshots
[Si aplica, agrega capturas de pantalla]

## Entorno
- Windows Version: [ej. Windows 11 22H2]
- .NET Version: [ej. 8.0.1]
- WingetUpdater Version: [ej. 1.0.0]
- Winget Version: [ejecuta `winget --version`]

## Información Adicional
[Cualquier otro contexto relevante]
```

---

## 💡 Proponer Nuevas Características

### Antes de Proponer
1. **Revisa el roadmap** - Verifica si ya está planeada
2. **Busca propuestas similares** - Puede haber una discusión existente
3. **Considera el alcance** - ¿La característica encaja con la visión del proyecto?

### Cómo Proponer una Característica

Crea un issue con esta estructura:

```markdown
## 🌟 Característica Propuesta
[Título claro y descriptivo]

## 🎯 Problema que Resuelve
[¿Qué problema de los usuarios soluciona esta característica?]

## 💡 Solución Propuesta
[Describe cómo funcionaría la característica]

## 🎨 Mockups/Ejemplos
[Si aplica, agrega imágenes o ejemplos de UI]

## 🔧 Alternativas Consideradas
[¿Hay otras formas de resolver el problema?]

## 📊 Beneficios
- Beneficio 1
- Beneficio 2

## ⚠️ Posibles Desventajas
- Desventaja 1
- Desventaja 2

## 🔍 Contexto Adicional
[Cualquier información relevante]
```

---

## 🔀 Pull Requests

### Proceso de PR

1. **Fork el repositorio**
   ```bash
   git clone https://github.com/YOUR_USERNAME/WPF-App.git
   cd WPF-App
   ```

2. **Crea una rama para tu feature**
   ```bash
   git checkout -b feature/nombre-descriptivo
   # o para bugs:
   git checkout -b fix/descripcion-del-bug
   ```

3. **Haz tus cambios**
   - Sigue las [guías de estilo](#-guías-de-estilo)
   - Escribe tests si es aplicable
   - Actualiza la documentación

4. **Commit con mensajes descriptivos**
   ```bash
   git commit -m "Add: Descripción clara del cambio"
   ```

5. **Push a tu fork**
   ```bash
   git push origin feature/nombre-descriptivo
   ```

6. **Abre un Pull Request**
   - Describe qué cambios hiciste y por qué
   - Referencia issues relacionados
   - Agrega screenshots si hay cambios visuales

### Checklist de PR

Antes de abrir un PR, asegúrate de que:

- [ ] El código compila sin errores
- [ ] No hay warnings críticos
- [ ] El código sigue las guías de estilo
- [ ] La documentación está actualizada
- [ ] Los cambios fueron probados localmente
- [ ] El commit tiene un mensaje descriptivo
- [ ] No hay archivos innecesarios (bin/, obj/, etc.)

### Estructura de Commits

Usa prefijos claros en tus commits:

- `Add:` - Nueva característica
- `Fix:` - Corrección de bug
- `Update:` - Actualización de código existente
- `Refactor:` - Refactorización sin cambiar funcionalidad
- `Docs:` - Cambios en documentación
- `Style:` - Cambios de formato (sin afectar código)
- `Test:` - Agregar o modificar tests
- `Perf:` - Mejoras de rendimiento

**Ejemplos:**
```
Add: Individual action buttons for each program
Fix: NullReferenceException when listing installed packages
Update: Microsoft Blue color palette for better accessibility
Refactor: Extract cleanup logic into separate service
Docs: Add contribution guidelines
```

---

## 🎨 Guías de Estilo

### Código C#

#### Naming Conventions
```csharp
// PascalCase para classes, methods, properties
public class WingetService { }
public void UpdatePackage() { }
public string PackageName { get; set; }

// camelCase para variables locales y parámetros
private string packageId;
public void ProcessPackage(string packageName) { }

// _camelCase para campos privados
private readonly WingetService _wingetService;

// UPPER_CASE para constantes
private const int MAX_RETRIES = 3;
```

#### Formato
- **Indentación:** 4 espacios (no tabs)
- **Llaves:** En nueva línea (estilo Allman)
- **Línea máxima:** 120 caracteres
- **Espacios:** Alrededor de operadores

```csharp
// ✅ Correcto
if (isValid)
{
    ProcessData();
}

// ❌ Incorrecto
if(isValid){
    ProcessData();
}
```

#### Async/Await
```csharp
// ✅ Usar async/await para operaciones asíncronas
public async Task<List<PackageInfo>> GetPackagesAsync()
{
    return await _service.FetchPackagesAsync();
}

// ✅ Sufijo "Async" en métodos asíncronos
public async Task UpdatePackageAsync(PackageInfo package)
{
    // Implementation
}
```

#### Comments y Documentation
```csharp
/// <summary>
/// Uninstalls a package using winget and removes residual files
/// </summary>
/// <param name="packageId">The unique identifier of the package</param>
/// <param name="options">Uninstallation options</param>
/// <returns>Result containing success status and removed files</returns>
public async Task<UninstallResult> UninstallCleanAsync(string packageId, UninstallOptions options)
{
    // Implementation
}
```

### XAML

#### Estructura
```xaml
<!-- Atributos en orden: Name, Style, Command, Properties, Events -->
<Button x:Name="UpdateButton"
        Style="{StaticResource ModernButton}"
        Command="{Binding UpdateCommand}"
        Content="Update"
        Margin="8,0,0,0"
        Click="UpdateButton_Click"/>
```

#### Naming
```xaml
<!-- PascalCase para nombres de controles -->
<Button x:Name="UpdateButton"/>
<TextBlock x:Name="StatusTextBlock"/>
```

### Comentarios

```csharp
// ✅ Buenos comentarios - Explican el "por qué"
// Use official Microsoft API to ensure compatibility with all Windows versions
int result = SHEmptyRecycleBin(IntPtr.Zero, null, flags);

// ❌ Malos comentarios - Explican el "qué" (obvio del código)
// Call the SHEmptyRecycleBin function
int result = SHEmptyRecycleBin(IntPtr.Zero, null, flags);
```

---

## 📁 Estructura del Proyecto

```
WingetUpdater/
├── Models/              # Data models (PackageInfo, etc.)
├── ViewModels/          # MVVM ViewModels
│   └── MainViewModel.cs
├── Views/              # XAML views
│   └── MainWindow.xaml
├── Services/           # Business logic services
│   ├── WingetService.cs
│   ├── CleanupService.cs
│   └── UninstallService.cs
├── Helpers/            # Utility classes
│   └── AdminHelper.cs
├── Commands/           # RelayCommand implementation
├── Converters/         # XAML value converters
└── Resources/
    └── Styles.xaml     # Application styles
```

### Agregar Nuevas Características

1. **Services** - Lógica de negocio va en `Services/`
2. **ViewModels** - Lógica de presentación en `ViewModels/`
3. **Views** - UI en XAML en `Views/` o `MainWindow.xaml`
4. **Models** - Clases de datos en `Models/`

---

## 🔧 Proceso de Desarrollo

### Setup del Entorno

1. **Clona el repositorio**
   ```bash
   git clone https://github.com/CharlieCardenasToledo/WPF-App.git
   cd WPF-App
   ```

2. **Abre en Visual Studio 2022** o **VS Code**
   - Visual Studio: Abre `WPF App.sln`
   - VS Code: Abre la carpeta raíz

3. **Restaura dependencias**
   ```bash
   cd WingetUpdater
   dotnet restore
   ```

4. **Compila el proyecto**
   ```bash
   dotnet build
   ```

5. **Ejecuta la aplicación**
   ```bash
   dotnet run
   ```

### Testing Local

Antes de hacer un PR, prueba:

1. **Compilación exitosa**
   ```bash
   dotnet build -c Release
   ```

2. **Funcionalidad básica**
   - Listar programas funciona
   - Actualizar un programa funciona
   - Desinstalar funciona
   - Logs se muestran correctamente

3. **Casos edge**
   - ¿Qué pasa sin conexión a internet?
   - ¿Qué pasa si winget no está instalado?
   - ¿Qué pasa al desinstalar un programa que no existe?

### Debugging

Para depurar la aplicación:

```bash
# Con Visual Studio: F5
# Con VS Code: F5 (asegúrate de tener configurado launch.json)
# CLI:
dotnet run --configuration Debug
```

---

## 🎯 Áreas Prioritarias para Contribuir

### Alta Prioridad
- [ ] **Sistema de pestañas** - Separar Actualizaciones | Programas | Limpieza
- [ ] **UI para limpieza del sistema** - Interface gráfica para CleanupService
- [ ] **Tests automatizados** - Unit tests y integration tests
- [ ] **Multiidioma** - Soporte para inglés, portugués, francés, etc.

### Media Prioridad
- [ ] **Historial de operaciones** - Log persistente de actualizaciones/desinstalaciones
- [ ] **Programación de actualizaciones** - Actualizar automáticamente a horas específicas
- [ ] **Themes** - Modo claro/oscuro
- [ ] **Búsqueda y filtros** - Buscar programas en la lista

### Mejoras de Código
- [ ] **Error handling** - Mejorar manejo de errores y mensajes al usuario
- [ ] **Performance** - Optimizar carga de lista de programas
- [ ] **Logging** - Sistema de logging más robusto
- [ ] **Configuración** - Permitir al usuario configurar rutas, opciones, etc.

---

## 📚 Recursos Útiles

### Documentación
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
- [Winget Documentation](https://docs.microsoft.com/en-us/windows/package-manager/winget/)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)

### Herramientas
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

---

## 💬 Comunicación

### ¿Preguntas?
- **Issues:** Para reportar bugs o proponer características
- **Discussions:** Para preguntas generales y discusiones

### Tiempo de Respuesta
- Los PRs reciben feedback usualmente en 2-5 días
- Los issues son revisados en 1-3 días
- Las discusiones se responden cuando sea posible

---

## 🙏 Agradecimientos

Gracias a todos los que contribuyen a hacer WingetUpdater mejor! 🎉

Cada contribución, sin importar el tamaño, es valiosa y apreciada.

---

## 📄 Licencia

Al contribuir a WingetUpdater, aceptas que tus contribuciones serán licenciadas bajo la [MIT License](LICENSE).

---

<div align="center">

**¿Listo para contribuir? ¡Adelante! 🚀**

[Reportar Bug](https://github.com/CharlieCardenasToledo/WPF-App/issues/new?labels=bug) • 
[Proponer Feature](https://github.com/CharlieCardenasToledo/WPF-App/issues/new?labels=enhancement) • 
[Hacer PR](https://github.com/CharlieCardenasToledo/WPF-App/compare)

</div>
