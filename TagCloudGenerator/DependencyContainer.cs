using System.Drawing;
using Autofac;

namespace TagCloudGenerator;

public class DependencyContainer : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TagCloudApplication>().AsSelf();
        builder.RegisterType<ConsoleClient>().AsSelf();
        
        builder.RegisterType<TextFileReader>().As<ITextReader>().SingleInstance();
        builder.RegisterType<FrequencyWordAnalyzer>().As<IWordAnalyzer>().SingleInstance();
        
        builder.RegisterType<TagProvider>().As<ITagProvider>().SingleInstance();
        builder.RegisterType<GraphicsTextMeasurer>().As<ITextMeasurer>().InstancePerDependency();
        builder.RegisterType<LinearFontSizeCalculator>().As<IFontSizeCalculator>().SingleInstance();
        
        builder.RegisterType<SpiralTagCloudAlgorithm>().As<ITagCloudArrangeAlgorithm>();
        builder.Register((c, p) =>
        {
            var settings = c.Resolve<AppSettings>();
            return new SpiralPointsProvider(settings.Center);
        }).As<ISpiralPointsProvider>();
        
        builder.Register<IColorScheme>((c, p) =>
        {
            var settings = c.Resolve<AppSettings>();
            return settings.ColorScheme switch
            {
                "Frequency" => new FrequencyBasedColorScheme(Color.LightGoldenrodYellow, Color.Orange),
                _ => new RandomColorScheme()
            };
        }).InstancePerDependency();
        
        builder.Register((c, p) =>
        {
            var center = p.TypedAs<Point>();
            var spiralProvider = new SpiralPointsProvider(center);
            return new CircularCloudLayouter(center, spiralProvider);
        }).As<ICloudLayouter>();
        
        builder.Register<Func<Point, ICloudLayouter>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return center => context.Resolve<ICloudLayouter>(new TypedParameter(typeof(Point), center));
        });
        
        builder.RegisterType<ImageSaver>().As<IImageSaver>().WithParameter("relativeOutputDirectory", "out").SingleInstance();
        builder.RegisterType<TagCloudVisualizer>().As<IVisualizer>().SingleInstance();
        
        builder.Register<IWordPreprocessor>(c =>
        {
            var settings = c.Resolve<AppSettings>();
            return new WordPreprocessor(settings.StopWords, settings.ToLowerCase);
        }).InstancePerDependency();
        
        builder.RegisterInstance(new AppSettings()).As<AppSettings>().ExternallyOwned();
    }
}