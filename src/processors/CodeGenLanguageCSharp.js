/**
 * Copyright (c) Facebook, Inc. and its affiliates.
 * @format
 * @flow
 */

'use strict';

const getBaseType = (type: string) => {
  return type.replace(/(^|^list\s*<\s*)([a-zA-Z0-9_\s]*)($|\s*>\s*$)/i, '$2');
};

const CodeGenLanguageCSharp = {
  formatFileName(clsName: {[x: string]: string}) {
    return clsName['name:pascal_case'] + '.cs';
  },

  specOverrideProcessing(APISpecs: {[x: string]: any}) {
    // Handling fields that are gated by capabilities
    // There fields will break requestAllFields in Java SDK
    for (const className in APISpecs) {
      const APIClsSpec = APISpecs[className];
      for (const fieldIndex in APIClsSpec.fields) {
        let fieldSpec = APIClsSpec.fields[fieldIndex];
        fieldSpec.not_visible |= fieldSpec['csharp:not_visible'];
      }
    }
    return APISpecs;
  },

  preMustacheProcess(
    APISpecs: {[x: string]: any},
    codeGenNameConventions: any,
    enumMetadataMap: {[x: string]: any},
  ) {
    let csBaseType: string;

    // Process APISpecs for Java
    // 1. type normalization
    // 2. enum type naming and referencing
    for (const clsName in APISpecs) {
      const APIClsSpec = APISpecs[clsName];
      for (const index in APIClsSpec.apis) {
        const APISpec = APIClsSpec.apis[index];
        for (const index2 in APISpec.params) {
          const paramSpec = APISpec.params[index2];
          /* when we have a param called 'params',
           * see GraphProductFeedRulesPost,
           * We will have two SetParams functions in the api. One is
           * to set all params for the api, which is of type
           * Map<String, Object>. Another is to set the individual parameter
           * named 'params'. If the type of the parameter is some kind of Map,
           * it will cause "have the same erasure" error in Java because
           * Java cannot distinguish different Map during runtime.
           * So we add a flag here to indicate the 'params' param and in
           * template, we generate it as SetParamParams to avoid conflict
           */
          if (paramSpec.name == 'params') {
            paramSpec.param_name_params = true;
          }
          if (['file', 'bytes', 'zipbytes'].indexOf(paramSpec.name) != -1) {
            APISpec.params[index2] = undefined;
            APISpec.allow_file_upload = true;
            continue;
          }
          if (paramSpec.type) {
            const baseType = getBaseType(paramSpec.type);
            if (enumMetadataMap[baseType]) {
              paramSpec.is_enum_param = true;
              const metadata = enumMetadataMap[baseType];
              if (!metadata.node) {
                if (!APIClsSpec.api_spec_based_enum_reference) {
                  APIClsSpec.api_spec_based_enum_reference = [];
                  APIClsSpec.api_spec_based_enum_list = {};
                }
                if (
                  !APIClsSpec.api_spec_based_enum_list[metadata.field_or_param]
                ) {
                  APIClsSpec.api_spec_based_enum_reference.push(metadata);
                  APIClsSpec.api_spec_based_enum_list[
                    metadata.field_or_param
                  ] = true;
                }
                csBaseType = 'Enum' + metadata['field_or_param:pascal_case'];
              } else {
                csBaseType =
                  metadata.node +
                  '.Enum' +
                  metadata['field_or_param:pascal_case'];
              }
              paramSpec['type:csharp'] = this.getTypeForCSharp(
                paramSpec.type.replace(baseType, csBaseType),
              );
              paramSpec['basetype:csharp'] = csBaseType;
            } else {
              paramSpec['type:csharp'] = this.getTypeForCSharp(paramSpec.type);
              if (paramSpec['type:csharp'] == 'String') {
                paramSpec.is_string = true;
              }
            }
          }
        }
        if (APISpec.params) {
          APISpec.params = APISpec.params.filter(element => element != null);
        }
      }

      for (const index in APIClsSpec.fields) {
        const fieldSpec = APIClsSpec.fields[index];
        const fieldCls = APISpecs[fieldSpec.type];
        if (fieldCls && fieldCls.has_get && fieldCls.has_id) {
          fieldSpec.is_root_node = true;
        }
        if (fieldSpec.type) {
          if (enumMetadataMap[fieldSpec.type]) {
            fieldSpec.is_enum_field = true;
          }
          const baseType = getBaseType(fieldSpec.type);
          if (APISpecs[baseType]) {
            fieldSpec.is_node = true;
            fieldSpec['csharp:node_base_type'] = this.getTypeForCSharp(baseType);
          }
          if (enumMetadataMap[baseType]) {
            const metadata = enumMetadataMap[baseType];
            csBaseType = 'Enum' + metadata['field_or_param:pascal_case'];
            fieldSpec['type:csharp'] = this.getTypeForCSharp(
              fieldSpec.type.replace(baseType, csBaseType),
            );
            if (!APIClsSpec.api_spec_based_enum_reference) {
              APIClsSpec.api_spec_based_enum_reference = [];
              APIClsSpec.api_spec_based_enum_list = {};
            }
            if (!APIClsSpec.api_spec_based_enum_list[metadata.field_or_param]) {
              APIClsSpec.api_spec_based_enum_reference.push(metadata);
              APIClsSpec.api_spec_based_enum_list[
                metadata.field_or_param
              ] = true;
            }
          } else {
            if (fieldSpec.keyvalue) {
              fieldSpec['type:csharp'] = 'List<KeyValue>';
            } else {
              fieldSpec['type:csharp'] = this.getTypeForCSharp(fieldSpec.type);
            }
          }
        }
        if (APIClsSpec['name:pascal_case'] === fieldSpec['name:pascal_case']) {
          fieldSpec['name:csharp'] = fieldSpec['name:pascal_case'] + '_';
        } else if (fieldSpec.is_irregular_name) {
          fieldSpec['name:csharp'] = 'Value' + fieldSpec['name:pascal_case'];
        } else {
          fieldSpec['name:csharp'] = fieldSpec['name:pascal_case'];
        }
      }
    }
    return APISpecs;
  },

  getTypeForCSharp(type: string): ?string {
    if (!type) {
      return null;
    }

    // This is not perfect. But it's working for all types we have so far.
    const typeMapping = {
      DateTime: /datetime/gi,
      ulong: /unsigned int/gi,
      bool: /bool(ean)?/gi,
      // long: /(((unsigned\s*)?(\bint|long)))(?![a-zA-Z0-9_])/gi,
      long: /\b(int|long)\b/,
      double: /(float|double)/gi,
      'List<$1>': /list\s*<\s*([a-zA-Z0-9_.<>,\s]*?)\s*>/g,
      '$1Dictionary<string, string>$2': /(^|<)map($|>)/i,
      'Dictionary<$1, $2>': /map\s*<\s*([a-zA-Z0-9_]*?)\s*,\s*([a-zA-Z0-9_<>]*?)\s*>/g,
    };

    let oldType;
    let newType = type;
    while (oldType !== newType) {
      oldType = newType;
      for (const replace in typeMapping) {
        newType = newType.replace(typeMapping[replace], replace);
      }
    }
    // This is to make a type named as list to JToken.
    // However the 'list' should not be preceden by any word,
    // which might be 'blacklist', and should not be replaced
    newType = newType.replace(/(?<!\w)list(?!<)/g, 'JToken');
    newType = newType.replace(/^file$/i, 'File');
    // List<file> => List<File>
    newType = newType.replace(/<file>/i, '<File>');
    return newType;
  },
  keywords: ['try', 'private', 'public', 'protected', 'internal', 'new', 'default', 'class'],
};

export default CodeGenLanguageCSharp;
