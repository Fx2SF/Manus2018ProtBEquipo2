using System;
using System.Windows.Forms;
using Google.Cloud.Vision.V1;
using System.IO;
using System.Drawing;
using ImageProcessor.Imaging.Formats;
using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WindowsFormsManus
{


    public partial class Form1 : Form
    {
       
        public List<String> fileNames = new List<string>();
        public string ImagePath { get; set; }
        public string Monto { get; set; }
        public string Cliente { get; set; }
        public string MontoEscrito { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string Fecha { get; set; }
        public string TipoCheque { get; set; }

        public int contador;
        OpenFileDialog fileDlg = new OpenFileDialog();

        public Form1()
        {
            InitializeComponent();
            btnConvertirImagen.Enabled = false;
            label1.Text = "";
            txtResultado.Text = " SELECCIONE CHEQUES ";

        }

        //*****Método que permite ingresar un registro de cheque a la BD*****//
         public void AgregarCheque(EntidadCheque cheque)
        {
            using (Model1 bd = new Model1())
            {
                var nuevoCheque = new Cheques();
                nuevoCheque.imagePath = cheque.ImagePath;
                nuevoCheque.monto = cheque.Monto;
                nuevoCheque.cliente = cheque.Cliente;
                nuevoCheque.montoEscrito = cheque.MontoEscrito;
                nuevoCheque.serie = cheque.Serie;
                nuevoCheque.numero = cheque.Numero;
                nuevoCheque.tipoCheque = cheque.TipoCheque;
                nuevoCheque.fecha = cheque.Fecha;
                bd.Cheques.Add(nuevoCheque);
                bd.SaveChanges();
            }
        }

         //*****Método que permite cargar múltiples archivos a procesar*****//
        private void btnCargarImagen_Click(object sender, EventArgs e)
        {
            btnConvertirImagen.Enabled = false;
         
            ImagePath = "";
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
               
                dlg.Multiselect = true;
                DialogResult dr = dlg.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                     
                   
                    // Lee cada archivo
                    foreach (String file in dlg.FileNames)
                    {
                       contador++;
                        ImagePath = file;
                        fileNames.Add(ImagePath);
                     
                      btnConvertirImagen.Enabled = true;
                       

                    }

                }
            }
        }

      

        private void Form1_Load(object sender, EventArgs e)
        {

        }

         //*****Método que permite limpiar el registro previa insersión a la BD*****//
        static string CleanInput(string strIn)
        {
            //Remplaza caracteres invalidos con srting vacíos.
            try
            {
                return Regex.Replace(strIn, @"[^\w\. $ ]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
           
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        //******Método que convierte a texto la imagen cargada*********//
        private void btnConvertirImagen_Click(object sender, EventArgs e)
        {

            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = contador;
           
            //Contadores
            int comercial = 0;
            int bbva = 0;
            foreach (string path in fileNames)
            {
                progressBar1.Refresh();
                label1.Refresh();
                
                label1.Text = this.progressBar1.Value + " de " + this.progressBar1.Maximum;

                this.progressBar1.Value++;
                progressBar1.Refresh();
                label1.Refresh();
                DetectLogo(path);
                label1.Text = this.progressBar1.Value + " de " + this.progressBar1.Maximum;

                if (TipoCheque.Contains("BBVA"))
                    
                {
                    EntidadCheque c = new EntidadCheque(path, DetectMontoBBVA(path), DetectClienteBBVA(path), DetectMontoEscritoBBVA(path), DetectSerieBBVA(path), DetectNumeroBBVA(path), "Cheque BANCO BBVA", DetectFechaBBVA(path));
                   AgregarCheque(c);

                   //Contador cheque bbva
                    bbva = bbva + 1;
                   
                }
                else
                {
                   
                     EntidadCheque c = new EntidadCheque(path, DetectMontoCom(path), DetectClienteCom(path), DetectMontoEscritoCom(path), DetectSerieCom(path), DetectNumeroCom(path), "Cheque BANCO COMERCIAL" ,DetectFechaCom(path));
                    AgregarCheque(c);
                    
                    //Contador cheque comercial
                     comercial = comercial + 1;
                }
            }
            txtResultado.Text = " CHEQUES PROCESADOS :" + Environment.NewLine + "Banco Comercial :\t" + comercial + Environment.NewLine + "Banco BBVA\t:" + bbva
               ;
          
        }
        //*****Detecta el logo del banco y discrimina el banco en el procesamiento previo al OCR****//
        private string DetectLogo(String selectedFile)
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
                            .Crop(new Rectangle(350, 0, 650, 200))
                             .Format(new PngFormat { Quality = 100 })
                            .Resolution(1024, 1024)
                            .Filter(MatrixFilters.BlackWhite)
                          
                            .Contrast(50)
                            .Tint(Color.White)
                           
                                //Se guardan en memoria los cambios
                              
                               .Save(outStream);
                    }
                    var ima = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(ima, ic);
                    TipoCheque = response.Text;
                    return TipoCheque;//MODI
                }
            }
        }
       

        /**********************************MÉTODOS CHEQUE BANCO COMERCIAL********************************************/
        //Detecta el monto y se aplica optimización de imagen
        private string DetectMontoCom(String selectedFile)
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
                                     .Crop(new Rectangle(991, 46, 357, 79))
                                     .Format(new PngFormat { Quality = 100 })
                                     .Filter(MatrixFilters.GreyScale)
                                     .Resolution(1024, 1024)
                                     .Contrast(70)
                                     .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                              
                    }
                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Monto = CleanInput(response.Text);
                        return Monto;
                    }
                    else
                    {
                        Monto = "Error de lectura";
                        return Monto;
                    }
                }
            }
        }

        //Detecta la fecha del cheque aplicando filtros de optimización de imagen
        private string DetectFechaCom(String selectedFile)
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
                                    .Resize(new Size(1402, 604))
                                    .Crop(new Rectangle(747, 139, 649, 87))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                              
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image, ic);
                    if (response != null)
                    {
                        Fecha = response.Text;
                        return Fecha;
                    }
                    else
                    {
                        Fecha = "Error de lectura";
                        return Fecha;
                    }
                }
            }
        }

        //Detecta el cliente y se aplican filtros para la optimización de imágenes
        private string DetectClienteCom(String selectedFile)
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
                               .Save(outStream);
                             
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var client = ImageAnnotatorClient.Create();
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Cliente = response.Text;
                        return Cliente;
                    }
                    else
                    {
                        Cliente = "Error de lectura";
                        return Cliente;
                    }
                }
            }
        }

        //Detecta el monto en manuscrito aplicando filtros para la optimización de la imagen
        private string DetectMontoEscritoCom(String selectedFile)
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
                              .Save(outStream);
                             
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);

                    if (response != null)
                    {
                        MontoEscrito = response.Text;
                        return MontoEscrito; //MODI
                    }
                    else
                    {
                        MontoEscrito = "Error de lectura";
                        return MontoEscrito;
                    }
                }
            }
        }

        //Detecta la serie del cheque aplicando filtros de optimización de imagen
        private string DetectSerieCom(String selectedFile)
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
                               .Save(outStream);
                           
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Serie = response.Text;
                        return Serie; //MODI
                    }
                    else
                    {
                        Serie = "Error de lectura";
                        return Serie;
                    }
                }
            }
        }

        //Detecta el número de serie del cheque aplicando filtros para la optimización de imagen
        private string DetectNumeroCom(String selectedFile)
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
                               .Save(outStream);
                               
                    }
                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Numero = response.Text;
                        return Numero; 
                    }
                    else
                    {
                        Numero = "Error de lectura";
                        return Numero; 
                    }
                }
            }
        }

        /*************************************************************************************************/
        /**********************************MÉTODOS CHEQUE BBVA********************************************/
        //Detecta el monto del cheque aplicando filtros de optimización de imagen
        private string DetectMontoBBVA(String selectedFile)
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
                               .Save(outStream);
                              
                    }

                    //Realiza la conversión con la imagen a texto a pertir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Monto = response.Text;
                        return Monto; 
                    }
                    else
                    {
                        Monto = "Error de lectura";
                        return Monto; 
                    }
                }


            }
        }

        //Detecta el cliente del cheque aplicando filtros de optimización de imagen
        private string DetectClienteBBVA(String selectedFile)
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
                               .Save(outStream);
                              
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image);
                    if (response != null)
                    {
                        Cliente = response.Text;
                        return Cliente; 
                    }
                    else
                    {
                        Cliente = "Error de lectura";
                        return Cliente; 
                    }
                }
            }
        }

        //Detecta la serie del cheque aplicando filtros de optimización de imagen
        private string DetectSerieBBVA(String selectedFile)
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
                                    .Crop(new Rectangle(48, 17, 39, 48))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.GreyScale)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                            
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image, ic);
                    if (response != null)
                    {
                        Serie = response.Text;
                        return Serie; 
                    }
                    else
                    {
                        Serie = "Error de lectura";
                        return Serie; 
                    }
                }
            }
        }
        //Detecta la numero del cheque aplicando filtros de optimización de imagen
        private string DetectNumeroBBVA(String selectedFile)
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
                                    .Crop(new Rectangle(78, 12, 70, 45))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.GreyScale)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                           
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image, ic);
                    if (response != null)
                    {
                        Numero = response.Text;
                        return Numero; 
                    }
                    else
                    {
                        Numero = "Error de lectura";
                        return Numero; 
                    }
                }
            }
        }
        //Detecta la monto escrito del cheque aplicando filtros de optimización de imagen
        private string DetectMontoEscritoBBVA(String selectedFile)
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
                                    .Crop(new Rectangle(10, 95, 504, 62))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                             
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image, ic);
                    if (response != null)
                    {
                        MontoEscrito = response.Text;
                        return MontoEscrito; 
                    }
                    else
                    {
                        MontoEscrito = "Error de lectura";
                        return MontoEscrito; 
                    }
                }
            }
        }

        //Detecta la fecha del cheque aplicando filtros de optimización de imagen
        private string DetectFechaBBVA(String selectedFile)
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
                                    .Crop(new Rectangle(533, 60, 158, 34))
                                    .Format(new PngFormat { Quality = 100 })
                                    .Resolution(1024, 1024)
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Tint(Color.White)


                               //Se guarda en memoria los cambios
                               .Save(outStream);
                            
                    }

                    //Realiza la conversión con la imagen a texto a partir de los cambios realizados
                    var image = Google.Cloud.Vision.V1.Image.FromStream(outStream);
                    var client = ImageAnnotatorClient.Create();
                    ImageContext ic = new ImageContext();
                    ic.LanguageHints.Add("es");
                    var response = client.DetectDocumentText(image, ic);
                    if (response != null)
                    {
                        Fecha = response.Text;
                        return Fecha; 
                    }
                    else
                    {
                        Fecha = "Error de lectura";
                        return Fecha; 
                    }
                }
            }
        }

        private void txtResultado_TextChanged(object sender, EventArgs e)
        {

        }

        private void picImagen_Click(object sender, EventArgs e)
        {

        }
        /***********************************************************************************************************/
        //codigo para el funcionamiento del progress bar
        int min = 0;    // Minimum value for progress range
        int max = 100;  // Maximum value for progress range
        int val = 0;        // Current progress
        Color BarColor = Color.AliceBlue;  // Color of progress meter
        Graphics g;
        SolidBrush brush;

        protected override void OnResize(EventArgs e)
        {
            // Invalidate the control to get a repaint.
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            g = e.Graphics;
            brush = new SolidBrush(BarColor);
            float percent = (float)(val - min) / (float)(max - min);
            Rectangle rect = this.ClientRectangle;

            // Calculate area for drawing the progress.
            rect.Width = (int)((float)rect.Width * percent);

            // Draw the progress meter.
            g.FillRectangle(brush, rect);

            // Draw a three-dimensional border around the control.
            Draw3DBorder(g);
        }

        public int Minimum
        {
            get
            {
                return min;
            }

            set
            {
                // Prevent a negative value.
                if (value < 0)
                {
                    min = 0;
                }

                // Make sure that the minimum value is never set higher than the maximum value.
                if (value > max)
                {
                    min = value;
                    min = value;
                }

                // Ensure value is still in range
                if (val < min)
                {
                    val = min;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Maximum
        {
            get
            {
                return max;
            }

            set
            {
                // Make sure that the maximum value is never set lower than the minimum value.
                if (value < min)
                {
                    min = value;
                }

                max = value;

                // Make sure that value is still in range.
                if (val > max)
                {
                    val = max;
                }

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        public int Value
        {
            get
            {
                return val;
            }

            set
            {
                int oldValue = val;

                // Make sure that the value does not stray outside the valid range.
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    val = max;
                }
                else
                {
                    val = value;
                }

                // Invalidate only the changed area.
                float percent;

                Rectangle newValueRect = this.ClientRectangle;
                Rectangle oldValueRect = this.ClientRectangle;

                // Use a new value to calculate the rectangle for progress.
                percent = (float)(val - min) / (float)(max - min);
                newValueRect.Width = (int)((float)newValueRect.Width * percent);

                // Use an old value to calculate the rectangle for progress.
                percent = (float)(oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

                Rectangle updateRect = new Rectangle();

                // Find only the part of the screen that must be updated.
                if (newValueRect.Width > oldValueRect.Width)
                {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                }
                else
                {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }

                updateRect.Height = this.Height;

                // Invalidate the intersection region only.
                this.Invalidate(updateRect);
            }
        }

        public Color ProgressBarColor
        {
            get
            {
                return BarColor;
            }

            set
            {
                BarColor = value;

                // Invalidate the control to get a repaint.
                this.Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int PenWidth = (int)Pens.White.Width;

            g.DrawLine(Pens.AliceBlue,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
            g.DrawLine(Pens.AliceBlue,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.Aqua,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                new Point(this.ClientRectangle.Width - PenWidth,
                this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.Aqua,
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth,
                    this.ClientRectangle.Height - PenWidth));
        }

        //***********************************************************************************************************************//
    }
}