# RichText
高效、支持大规模头顶文字

需求：我们希望能支持大规模头顶文字功能，有图文混排、图文并排、进度条等功能。
我们希望drawcall尽量的少，我们不希望其中一个子节点改变就会导致大量重计算。

常用解决方案：
1、创建两个Canvas，一个font，一个Image。但并不能有效的解决上述问题。
2、利用TextMeshPro，但TextMeshPro对atlas支持不友好，对字体库缺少文字支持不友好。

综上所述：
RichText有效的解决了上面的问题。我们依赖unity的高效动态合并功能。
RichText支持UI模式和3D模式。
