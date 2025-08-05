# DSW2025 TPI - Backend

## Comisión 3K2

## Integrantes

* Santiago Ezequiel Valdez (52676)
* Valentina Bugeau (53133)

---

## Configuración y despliegue local

Para preparar el entorno de desarrollo y ejecutar la API en su máquina local, siga estos pasos:

1. **Requisitos previos**

   * .NET 8 SDK (disponible en [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0))
   * SQL Server (instalado localmente o mediante contenedor Docker)
   * (Opcional) IDE: Visual Studio 2022 o Visual Studio Code con extensiones C#

2. **Clonación del repositorio**
   Exporte el proyecto desde el control de versiones a su equipo:

   ```bash
   git clone https://github.com/tu-usuario/Dsw2025Tpi-development.git
   cd Dsw2025Tpi-development
   ```

3. **Actualización de cadenas de conexión y configuración de JWT**
   En el archivo `Dsw2025Tpi.Api/appsettings.json`, reemplace los valores de ejemplo por los de su entorno:

   ```jsonc
   "ConnectionStrings": {
     "DomainConnection": "Server=TU_SERVIDOR;Database=Dsw2025Domain;Trusted_Connection=True;",
     "AuthConnection":   "Server=TU_SERVIDOR;Database=Dsw2025Auth;Trusted_Connection=True;"
   },
   "JwtConfig": {
     "Key": "<ClaveSecreta_Segura>",
     "Issuer": "Dsw2025Issuer",
     "Audience": "Dsw2025Audience",
     "ExpireInMinutes": 60
   }
   ```

   * **DomainConnection**: cadena de conexión para el contexto de dominio (entidades de negocio).

   * **AuthConnection**: cadena de conexión para el contexto de autenticación (Identity).

   * **JwtConfig**: parámetros de emisión y validación de tokens JWT.

   > **Seguridad:** Asegúrese de agregar `appsettings.Development.json` a `.gitignore` para evitar exponer claves sensibles de JWT. Solo incluya `appsettings.json` en el control de versiones con valores dummy.

4. **Aplicación de migraciones y poblado inicial de datos**
   Ejecute las migraciones de Entity Framework Core y permita que el seeder inserte datos de ejemplo:

   ```bash
   # Migración del esquema de dominio
   dotnet ef database update \
     --project Dsw2025Tpi.Data \
     --startup-project Dsw2025Tpi.Api \
     --context DomainContext

   # Migración del esquema de autenticación
   dotnet ef database update \
     --project Dsw2025Tpi.Data \
     --startup-project Dsw2025Tpi.Api \
     --context AuthenticateContext
   ```

   > **Nota:** Al iniciar la aplicación, `AppDataSeeder` ejecuta la inserción de roles, usuarios de prueba, productos y órdenes de ejemplo.

5. **Ejecución de la API**
   Inicie el host ASP.NET Core:

   ```bash
   cd Dsw2025Tpi.Api
   dotnet run
   ```

   * Accesos: `https://localhost:5001` y `http://localhost:5000`.
   * En entorno **Development**, la documentación Swagger estará disponible en `https://localhost:5001/swagger`.

---

## Endpoints disponibles

A continuación se detallan los recursos REST expuestos por la API, el método HTTP asociado y los requisitos de autenticación:

| Operación                   | Método | Ruta                        | Autenticación | Descripción                                                                                     |
| --------------------------- | ------ | --------------------------- | ------------- | ----------------------------------------------------------------------------------------------- |
| **Registrar usuario**       | POST   | `/api/auth/register`        | No            | Crea un nuevo usuario en Identity y genera la entidad `Customer` correspondiente en el dominio. |
| **Autenticar usuario**      | POST   | `/api/auth/login`           | No            | Valida credenciales y retorna un token JWT para uso en peticiones subsecuentes.                 |
| **Listar productos**        | GET    | `/api/products`             | Sí            | Obtiene la colección completa de productos registrados.                                         |
| **Obtener un producto**     | GET    | `/api/products/{id}`        | Sí            | Recupera los detalles de un producto por su identificador único.                                |
| **Crear producto**          | POST   | `/api/products`             | Sí            | Inserta un nuevo producto en el catálogo.                                                       |
| **Actualizar producto**     | PUT    | `/api/products/{id}`        | Sí            | Modifica los atributos de un producto existente.                                                |
| **Alternar estado activo**  | PATCH  | `/api/products/{id}/active` | Sí            | Cambia el indicador `IsActive` para habilitar o deshabilitar un producto.                       |
| **Listar órdenes**          | GET    | `/api/orders`               | Sí            | Devuelve órdenes con filtros opcionales (`customerId`, `status`, paginación).                   |
| **Obtener orden**           | GET    | `/api/orders/{id}`          | Sí            | Muestra los detalles de una orden específica, incluyendo ítems y estado actual.                 |
| **Crear orden**             | POST   | `/api/orders`               | Sí            | Registra una nueva orden, descuenta stock y genera las líneas de pedido correspondientes.       |
| **Actualizar estado orden** | PATCH  | `/api/orders/{id}/status`   | Sí            | Modifica el campo `OrderStatus` para reflejar el avance en el ciclo de vida de la orden.        |

### Uso de los endpoints

1. **Registro** → `POST /api/auth/register` con un payload JSON que incluya usuario, contraseña y datos de cliente.
2. **Login** → `POST /api/auth/login` para recibir el token JWT (Bearer).
3. Incluir encabezado HTTP `Authorization: Bearer {token}` en todas las solicitudes a rutas que requieran autenticación.

### Probar con Swagger UI

4. **Abrir Swagger UI**

   * En su navegador, navegue a `https://localhost:5001/swagger`.
   * Verá la página de documentación generada automáticamente con todos los endpoints organizados por controladores.

5. **Autenticación en Swagger**

   * Haga clic en el botón **Authorize** (ícono de candado) en la esquina superior derecha.
   * En el cuadro **Available authorizations**, verá un campo `bearerAuth`.
   * Ingrese:

     ```text
     Bearer <token>
     ```

     donde `<token>` es el JWT obtenido con el endpoint `/api/auth/login`.
   * Pulse **Authorize** y luego **Close**.
   * A partir de ahora, todas las peticiones incluirán automáticamente el encabezado `Authorization: Bearer <token>`.

6. **Ejecutar una petición**

   * Localice el endpoint que desee probar (por ejemplo, `POST /api/orders`).
   * Haga clic en el nombre del endpoint para expandirlo.
   * Seleccione **Try it out** para activar los campos editables.
   * Complete los parámetros de ruta, query o el body JSON según el modelo mostrado.
   * Pulse **Execute**.

7. **Interpretar resultados**

   * **Request URL & Headers**: podrá revisar la URL usada y los encabezados enviados (incluyendo `Authorization`).
   * **Response Code**: código de estado HTTP (200, 201, 400, 401, 404, etc.).
   * **Response Body**: JSON devuelto por la API con los datos de la operación.
   * **Response Headers**: encabezados HTTP de la respuesta.

8. **Obtener ejemplos de código**

   * Bajo la sección **Curl** o **HTTP Request**, Swagger UI genera un snippet que puede copiar para ejecutar la misma petición desde la línea de comandos o scripts.

9. **Validación de permisos**

   * Desautorice (botón **Logout**) y pruebe un endpoint protegido; debe recibir **401 Unauthorized**.
   * Vuelva a autorizar con un token válido para continuar.

10. **Actualización dinámica**

* Cada vez que modifique controladores o DTOs, reinicie la aplicación con `dotnet run`.
* Actualice la página de Swagger (F5) para cargar el nuevo esquema de la API.

> **Tip:** Aproveche Swagger UI para explorar esquemas, validar contratos y generar clientes HTTP automáticamente.
