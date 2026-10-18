CREATE DATABASE Gestion_Requisiciones;
GO
USE Gestion_Requisiciones;
GO

CREATE TABLE Rol (Id_Rol INT PRIMARY KEY, Rol VARCHAR(20) NOT NULL);
CREATE TABLE Departamentos (Id_Departamento INT PRIMARY KEY, Departamento VARCHAR(20) NOT NULL);
CREATE TABLE Estados_Empleados (Id_EstadoE INT PRIMARY KEY, EstadoE VARCHAR(20) NOT NULL);
CREATE TABLE Estados_Requisicion (Id_EstadoR INT PRIMARY KEY, EstadoR VARCHAR(20) NOT NULL);

CREATE TABLE Empleados
(
    Id_Empleado     INT PRIMARY KEY,
    Nombre          VARCHAR(30) NOT NULL,
    Apellido        VARCHAR(30) NOT NULL,
    Cargo           VARCHAR(30) NOT NULL,
    Id_Departamento INT NOT NULL REFERENCES Departamentos(Id_Departamento),
    Id_Rol          INT NOT NULL REFERENCES Rol(Id_Rol),
    Id_EstadoE      INT NOT NULL REFERENCES Estados_Empleados(Id_EstadoE)
);

CREATE TABLE Usuarios
(
    Id_Usuario  INT PRIMARY KEY,
    usuario     VARCHAR(50) NOT NULL,
    Contraseña  VARCHAR(50) NOT NULL,
    Id_Empleado INT NOT NULL REFERENCES Empleados(Id_Empleado)
);

CREATE TABLE Requisicion
(
    Id_Requisicion INT PRIMARY KEY,
    fecha          DATE NOT NULL,
    Id_Empleado    INT NOT NULL REFERENCES Empleados(Id_Empleado),
    Id_EstadoR     INT NOT NULL REFERENCES Estados_Requisicion(Id_EstadoR),
    Subtotal       DECIMAL(10,2) NOT NULL DEFAULT 0,
    Impuesto       DECIMAL(10,2) NOT NULL DEFAULT 0,
    Total          DECIMAL(10,2) NOT NULL DEFAULT 0
);

CREATE TABLE Detalle_Requisicion
(
    Id_Detalle      INT PRIMARY KEY,
    Id_Requisicion  INT NOT NULL REFERENCES Requisicion(Id_Requisicion),
    Descripcion     VARCHAR(200) NOT NULL,
    Precio_Unitario DECIMAL(10,2) NOT NULL CHECK (Precio_Unitario > 0),
    Cantidad        INT NOT NULL CHECK (Cantidad > 0)
);
GO

-- Vistas
CREATE VIEW dbo.VistaReportes
AS
SELECT
    b.Id_Requisicion AS Codigo, a.Nombre, a.Apellido, b.fecha AS Fecha,
    ISNULL(b.Subtotal, 0) AS Subtotal, ISNULL(b.Impuesto, 0) AS Impuesto,
    ISNULL(b.Total, 0) AS Total, e.EstadoR AS EstadoReporte
FROM dbo.Requisicion AS b
INNER JOIN dbo.Empleados AS a ON a.Id_Empleado = b.Id_Empleado
INNER JOIN dbo.Estados_Requisicion AS e ON e.Id_EstadoR = b.Id_EstadoR;
GO

CREATE VIEW dbo.VistaDetalleReporte
AS
SELECT
    d.Id_Requisicion AS Codigo, d.Id_Detalle, d.Descripcion, d.Cantidad,
    d.Precio_Unitario, d.Cantidad * d.Precio_Unitario AS Importe
FROM dbo.Detalle_Requisicion AS d;
GO

-- Procedimientos
CREATE PROCEDURE dbo.RequisicionesCrear
    @Id_Requisicion INT, @fecha DATE, @Id_Empleado INT, @Id_EstadoR INT,
    @Subtotal DECIMAL(10,2), @Impuesto DECIMAL(10,2), @Total DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Requisicion (Id_Requisicion, fecha, Id_Empleado, Id_EstadoR, Subtotal, Impuesto, Total)
    VALUES (@Id_Requisicion, @fecha, @Id_Empleado, @Id_EstadoR, @Subtotal, @Impuesto, @Total);
END
GO

CREATE PROCEDURE dbo.RequisicionesDetalleCrear
    @Id_Detalle INT, @Id_Requisicion INT, @Descripcion VARCHAR(200),
    @Precio_Unitario DECIMAL(10,2), @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Detalle_Requisicion (Id_Detalle, Id_Requisicion, Descripcion, Precio_Unitario, Cantidad)
    VALUES (@Id_Detalle, @Id_Requisicion, @Descripcion, @Precio_Unitario, @Cantidad);
END
GO

CREATE PROCEDURE dbo.RequisicionesCambiarEstado
    @Id_Requisicion INT, @Id_EstadoR INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Requisicion WHERE Id_Requisicion = @Id_Requisicion)
        THROW 50001, 'La requisición no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Requisicion WHERE Id_Requisicion = @Id_Requisicion AND Id_EstadoR IN (3, 4, 5))
        THROW 50002, 'La requisición está en un estado final y no puede modificarse.', 1;
    UPDATE dbo.Requisicion SET Id_EstadoR = @Id_EstadoR WHERE Id_Requisicion = @Id_Requisicion;
END
GO

-- Datos de prueba
INSERT INTO Rol VALUES (1,'Administrador'),(2,'Solicitante'),(3,'Supervisor'),(4,'Gerente'),(5,'Compras');
INSERT INTO Departamentos VALUES (1,'Informatica'),(2,'Contabilidad'),(3,'Recursos Humanos'),(4,'Administracion'),(5,'Operaciones');
INSERT INTO Estados_Empleados VALUES (1,'Activo'),(2,'Inactivo');
INSERT INTO Estados_Requisicion VALUES (1,'Creada'),(2,'En Revision'),(3,'Aprobada'),(4,'Rechazada'),(5,'Cancelada');

INSERT INTO Empleados (Id_Empleado, Nombre, Apellido, Cargo, Id_Departamento, Id_Rol, Id_EstadoE) VALUES
(1,'Carlos','Rodriguez','Administrador',1,1,1),(2,'Maria','Perez','Analista',2,2,1),
(3,'Jose','Martinez','Supervisor',3,3,1),(4,'Ana','Garcia','Gerente',4,4,1),
(5,'Luis','Hernandez','Encargado de Compras',5,5,1),(6,'Laura','Diaz','Tecnica',1,2,1),
(7,'Pedro','Sanchez','Asistente',2,2,2),(8,'Angel','Hilario','Administrador',1,1,1);

INSERT INTO Usuarios (Id_Usuario, usuario, Contraseña, Id_Empleado) VALUES
(1,'crodriguez','Carlos123',1),(2,'mperez','Maria123',2),(3,'jmartinez','Jose123',3),
(4,'agarcia','Ana123',4),(5,'lhernandez','Luis123',5),(6,'ldiaz','Laura123',6),
(7,'psanchez','Pedro123',7),(8,'Angel','12345',8);

INSERT INTO Requisicion (Id_Requisicion, fecha, Id_Empleado, Id_EstadoR) VALUES
(1,'2026-10-01',2,3),(2,'2026-10-03',3,2),(3,'2026-10-05',4,3),(4,'2026-10-08',5,1),
(5,'2026-10-10',6,4),(6,'2026-10-12',2,3),(7,'2026-10-14',3,2);

INSERT INTO Detalle_Requisicion (Id_Detalle, Id_Requisicion, Descripcion, Precio_Unitario, Cantidad) VALUES
(1,1,'Teclados USB',500.00,5),(2,1,'Mouse USB',350.00,5),(3,2,'Resmas de papel',250.00,10),
(4,2,'Lapices',15.00,50),(5,3,'Monitores de 24 pulgadas',8500.00,2),(6,3,'Cables HDMI',300.00,4),
(7,4,'Cartuchos de tinta',1800.00,3),(8,5,'Sillas de oficina',4500.00,4),
(9,6,'Memorias USB de 64 GB',450.00,10),(10,6,'Discos SSD de 1 TB',4200.00,2),(11,7,'Cuadernos',100.00,20);
GO

-- Totales de los datos de prueba (18 %)
UPDATE r
   SET Subtotal = x.Sub, Impuesto = ROUND(x.Sub * 0.18, 2), Total = ROUND(x.Sub * 1.18, 2)
  FROM dbo.Requisicion AS r
 CROSS APPLY (SELECT SUM(d.Cantidad * d.Precio_Unitario) AS Sub
                FROM dbo.Detalle_Requisicion AS d WHERE d.Id_Requisicion = r.Id_Requisicion) AS x;
GO

SELECT * FROM dbo.VistaReportes;
SELECT * FROM dbo.VistaDetalleReporte WHERE Codigo = 1;
GO
