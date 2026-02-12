# 🎓 AcademicManager - Sistema de Gestión Académica

Plataforma integral desarrollada en **Blazor Server (.NET 10)** usando **Clean Architecture** y **Dapper**.

## 🚀 Tecnologías

* **Frontend**: Blazor Server (Interactive Server Mode)
* **Backend**: .NET 10 Class Libraries
* **Base de Datos**: SQL Server
* **ORM**: Dapper (Micro-ORM de alto rendimiento)
* **Diseño**: CSS Moderno con Glassmorphism, temas oscuros y animaciones fluidas.

## 🏗️ Arquitectura (Clean Architecture)

La solución es un **Monorepo** dividido en capas:

1. **src/AcademicManager.Domain**: Entidades y reglas de negocio.
2. **src/AcademicManager.Application**: Interfaces, Servicios y Casos de Uso.
3. **src/AcademicManager.Infrastructure**: Implementación de Repositorios (Dapper) y acceso a datos.
4. **src/AcademicManager.Web**: Capa de Presentación (Blazor Components & Pages).

## 🛠️ Configuración e Instalación

### 1. Base de Datos

Ejecuta el script de inicialización en tu servidor SQL Server local:
`database/init.sql`

Esto creará la base de datos `AcademicManagerDB` y poblará datos de prueba (seed data).

### 2. Cadena de Conexión

Verifica `src/AcademicManager.Web/appsettings.json`. Por defecto está configurado para `localhost` con autenticación de Windows:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AcademicManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Ejecutar la Aplicación

Desde la raíz del repositorio:

```bash
dotnet run --project src/AcademicManager.Web/AcademicManager.Web.csproj
```

## 🔐 Credenciales de Acceso

El sistema incluye usuarios de prueba preconfigurados. **Consulta el archivo `CREDENTIALS.md` para más información** (no incluido en el repositorio público por seguridad).

> ⚠️ **IMPORTANTE**: Cambia todas las contraseñas por defecto antes de desplegar en producción.

## 📦 Módulos Incluidos

* **Dashboard**: Métricas en tiempo real.
* **Periodos**: Gestión de años/semestres académicos.
* **Grados y Secciones**: Configuración de niveles educativos.
* **Alumnos**: Registro completo con datos de apoderados.
* **Docentes**: Gestión de plana docente y perfiles.
* **Cursos**: Catálogo de materias por grado.
* **Horarios**: Asignación de aulas, docentes y tiempos.
* **Planificaciones**: Módulo para que los docentes suban sus planes de clase.

---
Desarrollado con ❤️ en .NET 10
