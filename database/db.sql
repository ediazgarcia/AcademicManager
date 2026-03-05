/*
 * SCRIPT DE INICIALIZACIÓN DE ACADEMICMANAGERDB SQL SERVER
 * ====================================================================================
 *
 */
USE master;
IF NOT EXISTS (
    SELECT *
    FROM sys.databases
    WHERE name = 'AcademicManagerDB'
) BEGIN CREATE DATABASE AcademicManagerDB;
END;
GO
USE AcademicManagerDB;
GO
/* ================== TABLAS ================== */
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'PeriodosAcademicos'
) BEGIN CREATE TABLE PeriodosAcademicos (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Nombre NVARCHAR(50) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Activo BIT NOT NULL DEFAULT 0,
    Tipo NVARCHAR(20),
    FechaCreacion DATETIME DEFAULT GETDATE()
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Grados'
) BEGIN CREATE TABLE Grados (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Nombre NVARCHAR(50) NOT NULL,
    Nivel NVARCHAR(20) NOT NULL,
    Orden INT NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Secciones'
) BEGIN CREATE TABLE Secciones (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Nombre NVARCHAR(10) NOT NULL,
    GradoId INT NOT NULL,
    Capacidad INT NOT NULL DEFAULT 30,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (GradoId) REFERENCES Grados(Id)
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Usuarios'
) BEGIN CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    NombreUsuario NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Rol NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100),
    UltimoAcceso DATETIME,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    DocenteId INT NULL,
    AlumnoId INT NULL,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret NVARCHAR(256) NULL
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Alumnos'
) BEGIN CREATE TABLE Alumnos (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Codigo NVARCHAR(20) NOT NULL UNIQUE,
    Nombres NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    FechaNacimiento DATE,
    Genero NVARCHAR(10),
    Direccion NVARCHAR(200),
    Telefono NVARCHAR(20),
    Email NVARCHAR(100),
    GradoId INT NULL,
    SeccionId INT NULL,
    NombreApoderado NVARCHAR(100),
    TelefonoApoderado NVARCHAR(20),
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (GradoId) REFERENCES Grados(Id),
    FOREIGN KEY (SeccionId) REFERENCES Secciones(Id)
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Docentes'
) BEGIN CREATE TABLE Docentes (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Codigo NVARCHAR(20) NOT NULL UNIQUE,
    Nombres NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    Especialidad NVARCHAR(100),
    GradoAcademico NVARCHAR(50),
    Telefono NVARCHAR(20),
    Email NVARCHAR(100),
    FechaNacimiento DATE,
    Genero NVARCHAR(10),
    Direccion NVARCHAR(200),
    FechaContratacion DATE,
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Cursos'
) BEGIN CREATE TABLE Cursos (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    Nombre NVARCHAR(100) NOT NULL,
    Codigo NVARCHAR(20) NOT NULL,
    Descripcion NVARCHAR(500),
    GradoId INT NOT NULL,
    HorasSemanales INT NOT NULL DEFAULT 2,
    Creditos INT NOT NULL DEFAULT 1,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (GradoId) REFERENCES Grados(Id)
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Horarios'
) BEGIN CREATE TABLE Horarios (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    CursoId INT NOT NULL,
    DocenteId INT NOT NULL,
    SeccionId INT NOT NULL,
    PeriodoAcademicoId INT NOT NULL,
    DiaSemana NVARCHAR(15) NOT NULL,
    HoraInicio TIME NOT NULL,
    HoraFin TIME NOT NULL,
    Aula NVARCHAR(20),
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CursoId) REFERENCES Cursos(Id),
    FOREIGN KEY (DocenteId) REFERENCES Docentes(Id),
    FOREIGN KEY (SeccionId) REFERENCES Secciones(Id),
    FOREIGN KEY (PeriodoAcademicoId) REFERENCES PeriodosAcademicos(Id)
);
END;
IF NOT EXISTS (
    SELECT *
    FROM sys.tables
    WHERE name = 'Planificaciones'
) BEGIN CREATE TABLE Planificaciones (
    Id INT PRIMARY KEY IDENTITY(1, 1),
    DocenteId INT NOT NULL,
    CursoId INT NOT NULL,
    SeccionId INT NOT NULL,
    PeriodoAcademicoId INT NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX),
    Objetivos NVARCHAR(MAX),
    Contenido NVARCHAR(MAX),
    Metodologia NVARCHAR(MAX),
    RecursosDidacticos NVARCHAR(MAX),
    Recursos NVARCHAR(MAX),
    Evaluacion NVARCHAR(MAX),
    Observaciones NVARCHAR(MAX),
    FechaClase DATE NOT NULL,
    FechaEnvio DATETIME DEFAULT GETDATE(),
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Borrador',
    AnoAcademico INT DEFAULT YEAR(GETDATE()),
    TipoplanificacionAcademico NVARCHAR(50) DEFAULT 'Anual',
    CriteriosEvaluacion NVARCHAR(MAX),
    UsuarioAprobadorId INT NULL,
    FechaAprobacion DATETIME NULL,
    MotivoRechazo NVARCHAR(MAX),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaActualizacion DATETIME NULL,
    FOREIGN KEY (DocenteId) REFERENCES Docentes(Id),
    FOREIGN KEY (CursoId) REFERENCES Cursos(Id),
    FOREIGN KEY (SeccionId) REFERENCES Secciones(Id),
    FOREIGN KEY (PeriodoAcademicoId) REFERENCES PeriodosAcademicos(Id)
);
END;
-- Module: Evaluacion, Grading, and Attendance
-- Creates tables for Rubrics (Evaluaciones), Grades (Calificaciones), and Attendance (Asistencias)

-- Evaluaciones (Rubrics/Assignments linked to a Course Planning)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Evaluaciones' AND xtype='U')
BEGIN
    CREATE TABLE Evaluaciones (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PlanificacionId INT NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(MAX),
        Peso DECIMAL(5,2) NOT NULL DEFAULT 0,
        MaximoPuntaje DECIMAL(5,2) NOT NULL DEFAULT 20,
        FechaEvaluacion DATETIME NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Evaluaciones_Planificaciones FOREIGN KEY (PlanificacionId) REFERENCES Planificaciones(Id)
    );
END;
GO

-- Calificaciones (Student Grades per Evaluation)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Calificaciones' AND xtype='U')
BEGIN
    CREATE TABLE Calificaciones (
        Id INT PRIMARY KEY IDENTITY(1,1),
        EvaluacionId INT NOT NULL,
        AlumnoId INT NOT NULL,
        Nota DECIMAL(5,2) NOT NULL DEFAULT 0,
        PuntosExtra DECIMAL(5,2) NOT NULL DEFAULT 0,
        Observacion NVARCHAR(MAX),
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Calificaciones_Evaluaciones FOREIGN KEY (EvaluacionId) REFERENCES Evaluaciones(Id),
        CONSTRAINT FK_Calificaciones_Alumnos FOREIGN KEY (AlumnoId) REFERENCES Alumnos(Id)
    );
END;
GO

-- Asistencias (Daily Attendance per Student per Class)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Asistencias' AND xtype='U')
BEGIN
    CREATE TABLE Asistencias (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PlanificacionId INT NOT NULL,
        AlumnoId INT NOT NULL,
        Fecha DATETIME NOT NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Presente',
        Observacion NVARCHAR(MAX),
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Asistencias_Planificaciones FOREIGN KEY (PlanificacionId) REFERENCES Planificaciones(Id),
        CONSTRAINT FK_Asistencias_Alumnos FOREIGN KEY (AlumnoId) REFERENCES Alumnos(Id)
    );
END;
GO

-- Module: User Registration Requests
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SolicitudesRegistro' AND xtype='U')
BEGIN
    CREATE TABLE SolicitudesRegistro (
        Id INT PRIMARY KEY IDENTITY(1,1),
        NombreUsuario NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        RolSolicitado NVARCHAR(20) NOT NULL,
        FechaSolicitud DATETIME DEFAULT GETDATE(),
        Estado NVARCHAR(20) DEFAULT 'Pendiente',
        Mensaje NVARCHAR(MAX)
    );
END
GO

-- Module: Tareas (Tasks)
IF NOT EXISTS (
    SELECT * FROM sys.tables WHERE name = 'Tareas'
) BEGIN
    CREATE TABLE Tareas (
        Id INT PRIMARY KEY IDENTITY(1, 1),
        PlanificacionId INT NULL,
        CursoId INT NOT NULL,
        PeriodoAcademicoId INT NOT NULL,
        DocenteId INT NOT NULL,
        Titulo NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(MAX),
        FechaPublicacion DATETIME DEFAULT GETDATE(),
        FechaEntrega DATETIME NOT NULL,
        PuntosMaximos INT NOT NULL DEFAULT 100,
        PermiteEntregaTardia BIT NOT NULL DEFAULT 0,
        DiasTardiosPermitidos INT NOT NULL DEFAULT 0,
        TipoArchivoPermitido NVARCHAR(500) DEFAULT 'pdf,doc,docx,xls,xlsx,ppt,pptx,jpg,jpeg,png,gif,zip',
        TamanoMaximoArchivo BIGINT DEFAULT 10485760,
        Activa BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (PlanificacionId) REFERENCES Planificaciones(Id),
        FOREIGN KEY (CursoId) REFERENCES Cursos(Id),
        FOREIGN KEY (PeriodoAcademicoId) REFERENCES PeriodosAcademicos(Id),
        FOREIGN KEY (DocenteId) REFERENCES Docentes(Id)
    );
END;
GO

-- Module: Entregas de Tareas
IF NOT EXISTS (
    SELECT * FROM sys.tables WHERE name = 'EntregasTareas'
) BEGIN
    CREATE TABLE EntregasTareas (
        Id INT PRIMARY KEY IDENTITY(1, 1),
        TareaId INT NOT NULL,
        AlumnoId INT NOT NULL,
        NombreArchivo NVARCHAR(255),
        RutaArchivo NVARCHAR(500),
        TipoArchivo NVARCHAR(100),
        TamanoArchivo BIGINT,
        Comentarios NVARCHAR(MAX),
        FechaEntrega DATETIME NOT NULL DEFAULT GETDATE(),
        EsTardia BIT NOT NULL DEFAULT 0,
        FechaCalificacion DATETIME NULL,
        Puntos INT NULL,
        Retroalimentacion NVARCHAR(MAX),
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (TareaId) REFERENCES Tareas(Id),
        FOREIGN KEY (AlumnoId) REFERENCES Alumnos(Id),
        UNIQUE (TareaId, AlumnoId)
    );
END;
GO

-- Module: Planificaciones Mensuales (Experta por Unidad/Mes)
IF NOT EXISTS (
    SELECT * FROM sys.tables WHERE name = 'PlanificacionesMensuales'
) BEGIN
    CREATE TABLE PlanificacionesMensuales (
        Id INT PRIMARY KEY IDENTITY(1, 1),
        PlanificacionId INT NULL,
        DocenteId INT NOT NULL,
        CursoId INT NOT NULL,
        SeccionId INT NOT NULL,
        PeriodoAcademicoId INT NOT NULL,
        Mes NVARCHAR(50) NOT NULL,
        NumeroMes INT NOT NULL DEFAULT 1,
        TituloUnidad NVARCHAR(255) NOT NULL,
        SituacionAprendizaje NVARCHAR(MAX),
        CompetenciasFundamentales NVARCHAR(MAX),
        CompetenciasEspecificas NVARCHAR(MAX),
        ContenidosConceptuales NVARCHAR(MAX),
        ContenidosProcedimentales NVARCHAR(MAX),
        ContenidosActitudinales NVARCHAR(MAX),
        IndicadoresLogro NVARCHAR(MAX),
        EstrategiasEnsenanza NVARCHAR(MAX),
        RecursosDidacticos NVARCHAR(MAX),
        ActividadesEvaluacion NVARCHAR(MAX),
        EjesTransversales NVARCHAR(MAX),
        Observaciones NVARCHAR(MAX),
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Borrador',
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (PlanificacionId) REFERENCES Planificaciones(Id),
        FOREIGN KEY (DocenteId) REFERENCES Docentes(Id),
        FOREIGN KEY (CursoId) REFERENCES Cursos(Id),
        FOREIGN KEY (SeccionId) REFERENCES Secciones(Id),
        FOREIGN KEY (PeriodoAcademicoId) REFERENCES PeriodosAcademicos(Id)
    );
END;
GO

-- Module: Planificaciones Diarias (Experta por Dia)
IF NOT EXISTS (
    SELECT * FROM sys.tables WHERE name = 'PlanificacionesDiarias'
) BEGIN
    CREATE TABLE PlanificacionesDiarias (
        Id INT PRIMARY KEY IDENTITY(1, 1),
        PlanificacionMensualId INT NOT NULL,
        Fecha DATE NOT NULL,
        IntencionPedagogica NVARCHAR(MAX),
        ActividadesInicio NVARCHAR(MAX),
        TiempoInicioMinutos INT NOT NULL DEFAULT 15,
        ActividadesDesarrollo NVARCHAR(MAX),
        TiempoDesarrolloMinutos INT NOT NULL DEFAULT 20,
        ActividadesCierre NVARCHAR(MAX),
        TiempoCierreMinutos INT NOT NULL DEFAULT 10,
        EstrategiasEnsenanza NVARCHAR(MAX),
        OrganizacionEstudiantes NVARCHAR(MAX),
        VocabularioDia NVARCHAR(MAX),
        Recursos NVARCHAR(MAX),
        LecturasRecomendadas NVARCHAR(MAX),
        Observaciones NVARCHAR(MAX),
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Borrador',
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (PlanificacionMensualId) REFERENCES PlanificacionesMensuales(Id)
    );
END;
GO
