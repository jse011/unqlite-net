下一步将开发ps脚本，进行多个 版本dll的自动编译，然后调用生成nuget

目前，nuget打包需要手动完成

1 在 bin/lib下放置4.0的dll
2 编译： nuget pack UnQLiteNet.csproj -Prop Configuration=Release -Prop Platform=x64
3 push之前，先进行手动的测试，解压到使用了包的地方，防止没有做第一步。自动化后，可以不用每次都检测了。