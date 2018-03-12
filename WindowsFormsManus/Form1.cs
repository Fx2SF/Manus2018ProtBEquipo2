using System;
using System.Windows.Forms;
using Google.Cloud.Vision.V1;
using System.IO;
using System.Drawing;
using ImageProcessor.Imaging.Formats;
using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using System.Linq;

namespace WindowsFormsManus
{


    public partial class Form1 : Form
    {
        public string ImagePath { get; set; }
        public string Monto { get; set; }
        public string Cliente { get; set; }
        public string MontoEscrito { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string TipoCheque { get; set; }

        OpenFileDialog fileDlg = new OpenFileDialog();

        public Form1()
        {
            InitializeComponent();
            btnConvertirImagen.Enabled = false;
        }

        //**Método que carga la imagen a convertir**//
        private void btnCargarImagen_Click(object sender, EventArgs e)
        {
            btnConvertirImagen.Enabled = false;
            txtResultado.Text = "";
            ImagePath = "";
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "jpeg files|*.jpg;*.JPG;*.png;*.PNG;*.tif;*.TIF;";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fileInfo = new FileInfo(dlg.FileName);
                    if (fileInfo.Length > 1024 * 1024 * 4)
                    {
                        MessageBox.Show("El tamaño de la imagen no puede ser mayor a 4MB");
                        return;
                    }
                    else
                    {
                        picImagen.Image = System.Drawing.Image.FromFile(dlg.FileName);
                        ImagePath = dlg.FileName;
                        lblInfo.Text = "Imagen cargada: " + fileInfo.Name;
                        lblInfo.BackColor = Color.LightGreen;

                        btnConvertirImagen.Enabled = true;
                    }
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        //**Método que convierte a texto la imagen a cargada**//
        private void btnConvertirImagen_Click(object sender, EventArgs e)
        {
            DetectLogo(ImagePath);

            if (TipoCheque.Contains("MER") || TipoCheque.Contains("BAN"))
            {
                DetectMontoCom(ImagePath);
                DetectClienteCom(ImagePath);
                DetectMontoEscritoCom(ImagePath);
                DetectSerieCom(ImagePath);
                DetectNumeroCom(ImagePath);
                txtResultado.Text = "*********Cheque BANCO COMERCIAL**********" + Environment.NewLine +
                "Serie: " + Serie + Environment.NewLine + "Numero: " + Numero + Environment.NewLine +
                "Monto: " + Monto + Environment.NewLine + "Cliente: " +
                Cliente + Environment.NewLine + "Monto Escrito: " + MontoEscrito;

            }
            else
            {

                DetectMontoBBVA(ImagePath);
                DetectClienteBBVA(ImagePath);
                //FALTA AGREGAR METODOS DE DETECCION DE DATOS
                txtResultado.Text = "*********Cheque BANCO BBVA**********" + Environment.NewLine +
                "Monto: " + Monto + Environment.NewLine + "Cliente: "+ Cliente;
            }

        }

        //Detecta el logo del banco y discrimina el banco en el procesamiento previo al OCR
        private void DetectLogo(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                           .Resize(new Size(1404, 608))
                            .Filter(MatrixFilters.BlackWhite)
                            .Crop(new Rectangle(350, 0, 611, 190))
                            .Contrast(50)
                            .Tint(Color.White)
                            .Format(new PngFormat { Quality = 100 })
                            .Resolution(1024, 1024)
                                //Se guardan en memoria los cambios
                                .Save(@"Debug\bin\imgTipoCheque.jpg")
                               .Save(outStream);
                    }
                    var ima = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var cli = ImageAnnotatorClient.Create();
                    var response = cli.DetectDocumentText(ima);
                    TipoCheque = response.Text;
                }
            }
        }

        //Detecta el monto y se aplica optimización de imagen
        private void DetectMontoCom(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                                   .Resize(new Size(1404, 608))
                                     .Crop(new Rectangle(991, 46, 357,79))
                                     .Format(new PngFormat { Quality = 100 })
                                     .Filter(MatrixFilters.GreyScale)
                                     .Resolution(1024, 1024)
                                     .Contrast(70)
                                     .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgModificadaMonto.jpg");
                    }
                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Monto = response.Text;
                    }
                    else { Monto = "Error de lectura"; }
                }
            }
        }

        //Detecta el cliente y se aplican filtros para la optimización de imágenes
        private void DetectClienteCom(String selectedFile)
        {
            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                                .Resize(new Size(1404, 608))
                                 .Crop(new Rectangle(29, 391, 645, 93))
                                 .Format(new PngFormat { Quality = 100 })
                                 .Resolution(1024, 1024)
                                 .Filter(MatrixFilters.GreyScale)
                                 .Contrast(70)
                                 .Tint(Color.White)

                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgModificadaCliente.jpg");
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Cliente = response.Text;
                    }
                    else { Cliente = "Error de lectura"; }
                }
            }
        }

        //Detecta el monto en manuscrito aplicando filtros para la optimización de la imagen
        private void DetectMontoEscritoCom(String selectedFile)
        {
            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                             .Resize(new Size(1404, 608))
                              .Crop(new Rectangle(35, 265, 1250, 60))
                              .Format(new PngFormat { Quality = 100 })
                              .Resolution(1024, 1024)
                              .Filter(MatrixFilters.GreyScale)
                              .Tint(Color.White)
                              .Contrast(70)
                              .Save(outStream)
                              .Save(@"Debug\bin\imgModificadaMontoEscrito.jpg");
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);

                    if (response != null)
                    {
                        MontoEscrito = response.Text;
                    }
                    else { MontoEscrito = "Error de lectura"; }
                }
            }
        }

        //Detecta la serie del cheque aplicando filtros de optimización de imagen
        private void DetectSerieCom(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                                    .Resize(new Size(1404, 608))
                                    .Crop(new Rectangle(1, 51, 118, 113))
                                    .Format(new PngFormat { Quality = 70 })
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Contrast(50)
                                    .Tint(Color.White)

                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgModificadaSerie.jpg");
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Serie = response.Text;
                    }
                    else { Serie = "Error de lectura"; }
                }
            }
        }

        //Detecta el número de serie del cheque aplicando filtros para la optimización de imagen
        private void DetectNumeroCom(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                            .Resize(new Size(1404, 608))
                             .Crop(new Rectangle(135, 50, 147, 73))
                             .Format(new PngFormat { Quality = 100 })
                             .Resolution(1024, 1024)
                             .Filter(MatrixFilters.GreyScale)
                             .Contrast(70)
                             .Tint(Color.White)

                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgModificadaNumero.jpg");
                    }
                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Numero = response.Text;
                    }
                    else { Numero = "Error de lectura"; }
                }
            }
        }

        //Detecta la monto del cheque aplicando filtros de optimización de imagen
        private void DetectMontoBBVA(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                                    .Resize(new Size(697, 289))
                                    .Crop(new Rectangle(477, 7, 214, 38))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.GreyScale)
                                    .Contrast(50)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgMontoBBVA.jpg");
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Monto = response.Text;
                    }
                    else { Monto = "Error de lectura"; }
                }


            }
        }

        //Detecta la monto del cheque aplicando filtros de optimización de imagen
        private void DetectClienteBBVA(String selectedFile)
        {

            var photoBytes = File.ReadAllBytes(selectedFile);

            //**Realiza procesamiento de imagen previo a la conversion**//
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(false))
                    {
                        //Filtros 
                        var img = imageFactory.Load(inStream)
                                    .Resize(new Size(697, 289))
                                    .Crop(new Rectangle(25, 186, 164, 52))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.GreyScale)
                                    .Contrast(50)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream)
                               .Save(@"Debug\bin\imgClienteBBVA.jpg");
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Cliente = response.Text;
                    }
                    else { Cliente = "Error de lectura"; }
                }
            }
        }

    }
}