using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;

namespace CajeroAutomatico
{
    //Se crea una clase usuario que almacena los datos 
    //Numero de Cuenta y el dinero disponible en la cuenta
    //Se agregan los getter y setter de los atributos 
    class Usuario
    {
        private int numeroDeCuenta;
        private long saldoActual;
        public int GetNumeroDeCuenta()
        {
            return numeroDeCuenta;
        }
        public void SetNumeroDeCuenta(int cuenta)
        {
            numeroDeCuenta = cuenta;
        }
        public void SetSaldoActual(long saldo)
        {
            saldoActual = saldo;
        }
        public long GetSaldoActual()
        {
            return saldoActual;
        }

    }

    //La clase cajero almacena el dinero fisico disponible para retirar en el cajero
    //Además de una instancia de la clase Usuario que se utliza para manejar los datos del Usuario
    //Contiene métodos para mostrar 3 menús: principal, de retiro, y el login
    class Cajero
    {
        private int moneyAmount = 10000;
        public Usuario currentUser = new Usuario();
        public void SetMoneyAmount(int amount)
        {
            moneyAmount = amount;
        }

        public int GetMoneyAmount()
        {
            return moneyAmount;
        }

        //Este metodo dibuja el login del cajero para acceder a los datos del usuario en la base de datos
        public void MostrarLogin()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ATM;";
            string query = "SELECT * FROM Cliente";
            //Se crea la conexion a la base de datos
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, databaseConnection);
            commandDatabase.CommandTimeout = 60;

            // Abre la base de datos
            try
            {
                databaseConnection.Open();
            }
            catch (Exception ex) //Maneja la excepcion si la base de datos está desconectada y la muestra en pantalla
            {
                Console.WriteLine(ex);
                Console.Read();
                return;
            }
            // Ejecuta la consulta almacenada en query
            MySqlDataReader reader = commandDatabase.ExecuteReader();

            int numeroDeIndentificacionPersonal;
            int numeroDeCuenta;
            Console.Clear();
            //Lee el número de cuenta y comprueba que sea de cinco digitos
            Console.WriteLine("Bienvenido");
            Console.Write("N° de Cuenta : ");
            numeroDeCuenta = Convert.ToInt32(Console.ReadLine());
            while (numeroDeCuenta > 99999 || numeroDeCuenta < 10000)
            {
                Console.Clear();
                Console.Write("Numero de Cuenta Inválido\n N° de Cuenta: ");
                numeroDeCuenta = Convert.ToInt32(Console.ReadLine());
            }
            //Lee el NIP y comprueba que sea de cinco digitos
            Console.Write("Introduzca su NIP: ");
            numeroDeIndentificacionPersonal = Convert.ToInt32(Console.ReadLine());
            while(numeroDeIndentificacionPersonal > 99999 || numeroDeIndentificacionPersonal <10000)
            {
                Console.Clear();
                Console.Write("NIP Inválido\n Introduzca su NIP: ");
                numeroDeIndentificacionPersonal = Convert.ToInt32(Console.ReadLine());
            }

            //Comprueba que los datos obtenidos por medio del teclado correspondan con algun registro en la base de datos
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    // En nuestra base de datos, el array row contiene:  NCuenta 0, NIP 1, Saldo 2
                    string[] row = { reader.GetString(0), reader.GetString(1), reader.GetString(2) };
                    if(numeroDeIndentificacionPersonal == Convert.ToInt64(row[1]) && numeroDeCuenta == Convert.ToInt64(row[0]))
                    {
                        //Si encuentra una coincidencia en la DB, modifica los atributos del objeto currentUser de la clase Usuario
                        currentUser.SetNumeroDeCuenta(numeroDeCuenta);
                        currentUser.SetSaldoActual(Convert.ToInt64(row[2]));
                        //Cierra la conexión a la base de datos
                        databaseConnection.Close();
                        MostrarMenuPrincipal();
                    }
                }
                //Si no encuentra coincidencias en la base de datos regresa un mensaje de error
                Console.WriteLine("El numero de cuenta o NIP ingresados son incorrectos");
                System.Threading.Thread.Sleep(2250);
                MostrarLogin();
            }
            else
            {
                //Si la base de datos está vacia, regresa este error
                Console.WriteLine("No se encontraron datos.");
            }
            //Cierra la conexión a la base de datos
            databaseConnection.Close();
        }
        
        //Este método dibuja el menú para escoger las opciones de retiro de efectivo
        public void MostrarMenuRetirar()
        {
            Console.Clear();
            ConsoleKeyInfo opcionRetiro;
            int montoRetiro = 0;
            Console.Clear();
            //Muestra las opciones posibles de retiro 
            Console.WriteLine("Presione el número correspondiente al monto que desea retirar \n[1] $20 \n[2] $40 \n[3] $60 \n[4] $100 \n[5] $200 \n[0] Cancelar Operación");
            opcionRetiro = Console.ReadKey(true);
            Console.Clear();
            //Dependiendo de la opcion elegida se asigna un valor a montoRetiro o se cancela la operación
            switch (opcionRetiro.KeyChar)
            {
                case '0':
                    Console.WriteLine("Operación Cancelada");
                    System.Threading.Thread.Sleep(750);
                    MostrarMenuPrincipal();
                    break;
                case '1':
                    montoRetiro = 20;
                    Console.WriteLine("Monto = 20");
                    break;
                case '2':
                    montoRetiro = 40;
                    break;
                case '3':
                    montoRetiro = 60;
                    break;
                case '4':
                    montoRetiro = 100;
                    break;
                case '5':
                    montoRetiro = 200;
                    break;
                default:
                    Console.WriteLine("Seleccione una opcion válida");
                    System.Threading.Thread.Sleep(750);
                    MostrarMenuRetirar();
                    break;

            }
            //Se prueba si el monto a retirar es mayor al saldo del usuario y arroja un error
            if(currentUser.GetSaldoActual() < montoRetiro)
            {
                Console.WriteLine("Fondos Insuficientes, seleccione un monto menor");
                System.Threading.Thread.Sleep(1000);
                MostrarMenuRetirar();
            }
            //Comprueba si el cajero tiene suficiente dinero físico para el retiro
            else if (montoRetiro > moneyAmount)
            {
                Console.WriteLine("El cajero no tiene suficiente efectivo, seleccione un monto menor");
                System.Threading.Thread.Sleep(1000);
                MostrarMenuRetirar();
            }
            //Si todo es correcto, el montoARetirar se resta al saldo del usuario y se entrega el dinero
            else 
            {
                currentUser.SetSaldoActual(currentUser.GetSaldoActual() - montoRetiro);
                moneyAmount -= montoRetiro;
                Console.WriteLine("Tome su dinero ↓");
                System.Threading.Thread.Sleep(4000);
                MostrarMenuPrincipal();
            }
            
        }

        //Este método dibuja el menú principal y nos permite manejar las opciones de operaciones dispinibles en el cajero
        public void MostrarMenuPrincipal()
        {
            ConsoleKeyInfo opcionMenu;
            //Se prepara la conexión a la base de datos
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ATM;";
            //Declaramos un string que almacena una consulta para actualizar los datos de la base de datos con los atributos del objeto currentUser;
            string query = "UPDATE Cliente SET Saldo = " + currentUser.GetSaldoActual() + " WHERE NCuenta = " + currentUser.GetNumeroDeCuenta() + ";";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, databaseConnection);
            commandDatabase.CommandTimeout = 60;

            // Abre la base de datos
            databaseConnection.Open();
            // Ejecuta la consulta
            MySqlDataReader reader = commandDatabase.ExecuteReader();
            //Se cierra la conexion
            databaseConnection.Close();
            Console.Clear();
            //Se muestra un menú con las operaciones disponibles del ATM
            Console.WriteLine("Presione el número de la operación que desea realizar \n[1] Solicitud de Saldo  \n[2] Retiro \n[3] Depósito \n[4] Salir" );
            opcionMenu = Console.ReadKey(true);
            Console.Clear();
            //Maneja las opciones del menu basado en la tecla presionada
            switch (opcionMenu.KeyChar)
            {
                case '1':
                    //El objeto currentUser contiene la informacion del usuario extraida de la base de datos
                    //Se muestra el saldo disponible, almacenado en el atributo saldoActual
                    Console.WriteLine("Solicitud de Saldo");
                    Console.WriteLine("Su saldo actual es de " + currentUser.GetSaldoActual());
                    Console.WriteLine("Presione una tecla para volver al Menu Principal");
                    Console.ReadKey(true);
                    break;
                case '2':
                    
                    Console.WriteLine("Retiro");
                    MostrarMenuRetirar();
                    break;
                case '3':
                    long montoDeposito;
                    //Se crea un nuevo timer con periodo de 2 minutos
                    Timer tiempoDeposito = new Timer(120000);
                    //Se desactiva el autoreinicio del timsr cuando termine
                    tiempoDeposito.AutoReset = false;
                    //Se asigna la funcion OnTimedEvent para manejar el evento del timer llegando a 0
                    tiempoDeposito.Elapsed += OnTimedEvent;
                    //Se ejecuta cuando el timer llega a cero
                    void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
                    {   
                        //Se muestra en pantalla un mensaje de error por 1.5 segundos y vuelve al menú principal
                        Console.WriteLine("Transacción cancelada por timeout");
                        System.Threading.Thread.Sleep(1500);
                        //Apagamos el timer
                        tiempoDeposito.Enabled = false;
                        MostrarMenuPrincipal();
                    }
                    //Se pide el monto de depósito en centavos y se almacena en montoDeposito
                    Console.WriteLine("Depósito");
                    Console.Write("Digite el monto de depósito en centavos: ");
                    montoDeposito = Convert.ToInt64(Console.ReadLine());
                    //Una vez ingresada la cantidad se inicia el timer
                    //Si el timer llega a 0, se ejecuta la función OnTimedEvent
                    tiempoDeposito.Enabled = true;
                    //La condicion dentro del while nos permite saber si el timer sigue corriendo
                    //Si sigue corriendo nos permite presionar una tecla (Ingresar el dinero)
                    //Si el timer ya llego a cero su propiedad Enabled sera false
                    while (tiempoDeposito.Enabled)
                    {
                        //Revisa si una tecla es presionada
                        if (Console.KeyAvailable == true)
                        {
                            //Si la tecla es presionada, la lee y  detiene el timer 
                            Console.ReadKey(true);
                            tiempoDeposito.Enabled = false;
                            //Modifica la propiedad saldo y le suma la cantidad depositada convertida a pesos
                            currentUser.SetSaldoActual(currentUser.GetSaldoActual() + montoDeposito / 100);
                            //Muestra un mensaje por un segundo y regresa al menú principal
                            Console.WriteLine("Depósito realizado con éxito");
                            System.Threading.Thread.Sleep(1000);


                            MostrarMenuPrincipal();
                        }
                    }
                    break;
                case '4':
                    //Vuelve al login
                    Console.WriteLine("Salir");
                    MostrarLogin();
                    break;
                default:
                    //Si se presiona una tecla que no corresponda a una de las funciones
                    //Se muestra un error por .75 segundos y vuelve a pedir una opcion
                    Console.WriteLine("\nDigite una opcion válida");
                    System.Threading.Thread.Sleep(750);
                    break;
            }
            MostrarMenuPrincipal();
        }

    }

    class Program
    {

        static void Main(string[] args)
        {
            //Crea un objeto de la clase Cajero
            Cajero c = new Cajero();
            //Llama  a la funcion MostrarLogin
            c.MostrarLogin();
        }
    }
}
