#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Nulls;
using Transformalize.Providers.File.Transforms;
using Transformalize.Transforms;
using Transformalize.Transforms.System;
using Transformalize.Validators;

namespace JunkDrawer.Autofac {
    public static class TransformFactory {

        public static IEnumerable<ITransform> GetTransforms(IComponentContext ctx, Process process, Entity entity, IEnumerable<Field> fields) {
            var transforms = new List<ITransform>();
            foreach (var f in fields.Where(f => f.Transforms.Any())) {
                var field = f;
                if (field.RequiresCompositeValidator()) {
                    transforms.Add(new CompositeValidator(
                        new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field),
                        field.Transforms.Select(t => ShouldRunTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t)))
                        ));
                } else {
                    transforms.AddRange(field.Transforms.Select(t => ShouldRunTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t))));
                }
            }
            return transforms;
        }

        public static ITransform ShouldRunTransform(IComponentContext ctx, IContext context) {
            return context.Operation.ShouldRun == null ? SwitchTransform(ctx, context) : new ShouldRunTransform(context, SwitchTransform(ctx, context));
        }

        static ITransform SwitchTransform(IComponentContext ctx, IContext context) {

            switch (context.Operation.Method) {

                case "add": case "sum": return new AddTransform(context);
                case "coalesce": return new CoalesceTransform(context);
                case "concat": return new ConcatTransform(context);
                case "connection": return new ConnectionTransform(context);
                case "convert": return new ConvertTransform(context);
                case "copy": return new CopyTransform(context);
                case "datepart": return new DatePartTransform(context);
                case "fileext": return new FileExtTransform(context);
                case "filename": return new FileNameTransform(context);
                case "filepath": return new FilePathTransform(context);
                case "format": return new FormatTransform(context);
                case "formatphone": return new Transformalize.Transforms.FormatPhoneTransform(context);
                case "hashcode": return new HashcodeTransform(context);
                case "htmldecode": return new DecodeTransform(context);
                case "insert": return new InsertTransform(context);
                case "invert": return new InvertTransform(context);
                case "join": return new JoinTransform(context);
                case "js": case "javascript": return ctx.ResolveNamed<ITransform>("js", new TypedParameter(typeof(PipelineContext), context));
                case "last": return new LastTransform(context);
                case "left": return new LeftTransform(context);
                case "lower": case "tolower": return new ToLowerTransform(context);
                case "map": return new MapTransform(context);
                case "match": return new RegexMatchTransform(context);
                case "multiply": return new MultiplyTransform(context);
                case "next": return new NextTransform(context);
                case "now": return new UtcNowTransform(context);
                case "padleft": return new PadLeftTransform(context);
                case "padright": return new PadRightTransform(context);
                case "razor": return ctx.ResolveNamed<ITransform>("razor", new TypedParameter(typeof(PipelineContext), context));
                case "regexreplace": return new RegexReplaceTransform(context);
                case "remove": return new RemoveTransform(context);
                case "replace": return new ReplaceTransform(context);
                case "right": return new RightTransform(context);
                case "splitlength": return new SplitLengthTransform(context);
                case "substring": return new SubStringTransform(context);
                case "tag": return new TagTransform(context);
                case "tostring": return new ToStringTransform(context);
                case "totime": return new ToTimeTransform(context);
                case "toyesno": return new ToYesNoTransform(context);
                case "trim": return new TrimTransform(context);
                case "trimend": return new TrimEndTransform(context);
                case "trimstart": return new TrimStartTransform(context);
                case "upper": case "toupper": return new ToUpperTransform(context);
                case "xmldecode": return new DecodeTransform(context);

                case "include": return new FilterTransform(context, FilterType.Include);
                case "exclude": return new FilterTransform(context, FilterType.Exclude);

                case "fromsplit": return new FromSplitTransform(context);
                case "fromlengths": return new FromLengthsTranform(context);

                // return true or false, validators
                case "any": return new AnyTransform(context);
                case "startswith": return new StartsWithTransform(context);
                case "endswith": return new EndsWithTransform(context);
                case "in": return new InTransform(context);
                case "contains": return new ContainsTransform(context);
                case "is": return new IsTransform(context);
                case "equal":
                case "equals": return new EqualsTransform(context);
                case "isempty": return new IsEmptyTransform(context);
                case "isdefault": return new IsDefaultTransform(context);

                default:
                    context.Warn("The {0} method is undefined.", context.Operation.Method);
                    return new NullTransform(context);
            }
        }
    }
}