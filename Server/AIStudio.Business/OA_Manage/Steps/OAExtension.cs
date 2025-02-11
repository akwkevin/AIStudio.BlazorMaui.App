﻿using AIStudio.Common.Service;
using AIStudio.Entity.Base_Manage;
using AIStudio.Entity.DTO.OA_Manage;
using AIStudio.IBusiness.Base_Manage;
using AIStudio.Util;
using AIStudio.Util.DiagramEntity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AIStudio.Business.OA_Manage.Steps
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public class OAExtension
    {
        /// <summary>初始化数据</summary>
        /// <param name="json"></param>
        /// <param name="id"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static string InitOAData(string json, string id)
        {
            var oaData = json.ToObject<OA_Data>();
            List<OA_Step> oASteps = new List<OA_Step>();
            if (oaData.Nodes.Count(p => p.Kind == NodeKinds.Start) != 1)
            {
                throw new Exception("开始节点的个数不等于1个");
            }

            if (oaData.Nodes.Count(p => p.Kind == NodeKinds.End) != 1)
            {
                throw new Exception("结束节点的个数不等于1个");
            }

            if (oaData.Nodes.Count(p => p.Kind == NodeKinds.COBegin) != oaData.Nodes.Count(p => p.Kind == NodeKinds.COEnd))
            {
                throw new Exception("并行节点的个数不是成对出现");
            }

            oaData.Id = id;
            oaData.DataType = StepType.Data;
            oaData.Steps = new List<OA_Step>();

            foreach (var node in oaData.Nodes)
            {
                OA_Step oAStep = new OA_Step();
                oAStep.Id = node.Id;
                oAStep.Label = node.Label;
                oAStep.StepType = KindToType(node.Kind);
                oAStep.ActRules = new ActRule();
                oAStep.ActRules.UserIds = node.UserIds?.ToList();
                oAStep.ActRules.RoleIds = node.RoleIds?.ToList();
                oAStep.ActRules.ActType = node.ActType;
                oASteps.Add(oAStep);
            }

            foreach (var edge in oaData.Links)
            {
                var source = oASteps.FirstOrDefault(p => p.Id == edge.SourceId);
                if (source != null)
                {
                    if (source.StepType == StepType.Decide)
                    {
                        source.SelectNextStep.Add(edge.TargetId, "data.Flag" + edge.Label);
                    }
                    else if (source.StepType == StepType.COBegin)
                    {
                        source.SelectNextStep.Add(edge.TargetId, "True");
                    }
                    else
                    {
                        source.NextStepId = edge.TargetId;
                    }
                }
            }

            var oAStartStep = oASteps.Single(p => p.StepType == StepType.Start);
            if (string.IsNullOrEmpty(oAStartStep.NextStepId))
            {
                throw new Exception("开始节点没有下一个节点");
            }
            oaData.Steps.Add(oAStartStep);
            oASteps.Remove(oAStartStep);

            string nextstepid = oAStartStep.NextStepId;
            oaData.Steps.AddRange(GetNextStep(oASteps, nextstepid));

            return JsonConvert.SerializeObject(oaData);
        }

        /// <summary>
        /// 获取下一个节点
        /// </summary>
        /// <param name="oASteps"></param>
        /// <param name="nextstepid"></param>
        /// <returns></returns>
        public static List<OA_Step> GetNextStep(List<OA_Step> oASteps, string nextstepid)
        {
            List<OA_Step> outsteps = new List<OA_Step>();
            List<string> nextids = new List<string>();
            var step = oASteps.FirstOrDefault(p => p.Id == nextstepid);
            if (step != null)
            {
                if (!string.IsNullOrEmpty(step.NextStepId))
                {
                    nextids.Add(step.NextStepId);
                }
                else if (step.SelectNextStep != null && step.SelectNextStep.Count > 0)
                {
                    nextids.AddRange(step.SelectNextStep.Keys);
                }

                outsteps.Add(step);
                oASteps.Remove(step);
            }

            int index = outsteps.IndexOf(step);

            nextids.Reverse();
            foreach (var next in nextids)
            {
                outsteps.InsertRange(index + 1, GetNextStep(oASteps, next));
            }
            return outsteps;
        }

        /// <summary>
        /// 名字转化成节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string KindToType(NodeKinds kind)
        {
            switch (kind)
            {
                case NodeKinds.Start: return StepType.Start;
                case NodeKinds.Middle: return StepType.Middle;
                case NodeKinds.End: return StepType.End;
                case NodeKinds.Decide: return StepType.Decide;
                case NodeKinds.COBegin: return StepType.COBegin;
                case NodeKinds.COEnd: return StepType.COEnd;
                default: return StepType.Normal;
            }
        }

        /// <summary>
        /// 初始化步骤
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<OA_Data> InitOAStep(OA_UserFormDTO data, IServiceProvider serviceProvider)
        {
            IBase_DepartmentBusiness _base_DepartmentBusiness = serviceProvider.GetRequiredService<IBase_DepartmentBusiness>();
            IBase_UserBusiness _base_UserBusiness = serviceProvider.GetRequiredService<IBase_UserBusiness>();
            IBase_RoleBusiness _base_RoleBusiness = serviceProvider.GetRequiredService<IBase_RoleBusiness>();
            IBase_UserRoleBusiness _base_UserRoleBusiness = serviceProvider.GetRequiredService<IBase_UserRoleBusiness>();
            Base_Department department = null;

            var userlist = await _base_UserBusiness.GetListAsync();
            var rolelist = await _base_RoleBusiness.GetListAsync();
            var userrolelist = await _base_UserRoleBusiness.GetListAsync();
            var departmentlist = await _base_DepartmentBusiness.GetListAsync();

            if (!string.IsNullOrEmpty(data.ApplicantDepartmentId))
            {
                department = departmentlist.FirstOrDefault(p => p.Id == data.ApplicantDepartmentId);
                if (department != null)
                {
                    data.ApplicantDepartment = department.Name;
                }
            }

            OA_Data oAData = data.WorkflowJSON.ToObject<OA_Data>();
            oAData.Flag = data.Flag;

            foreach (var step in oAData.Steps)
            {
                //恢复指向上一个节点
                if (!string.IsNullOrEmpty(step.NextStepId))
                {
                    var nextstep = oAData.Steps.FirstOrDefault(p => p.Id == step.NextStepId);
                    if (nextstep == null)
                        throw new Exception(string.Format("流程异常，无法找到{0}的下一个流程节点{1}", step.Id, step.NextStepId));
                    if (nextstep.StepType == StepType.Decide)
                    {
                        var nsteps = oAData.Steps.Where(p => nextstep.SelectNextStep.Any(q => p.Id == q.Key));
                        if (nsteps == null || nsteps.Count() == 0)
                            throw new Exception(string.Format("流程异常，无法找到{0}的下一个流程节点{1}", step.Id, step.NextStepId));

                        //跳过Decide指向下面的子节点
                        foreach (var nstep in nsteps)
                        {
                            if (nstep.PreStepId == null)
                                nstep.PreStepId = new List<string>();
                            nstep.PreStepId.Add(step.Id);
                        }
                    }
                    else
                    {
                        if (nextstep.PreStepId == null)
                            nextstep.PreStepId = new List<string>();
                        nextstep.PreStepId.Add(step.Id);
                    }
                }
                //查找审批人
                if (step.ActRules?.UserIds != null && step.ActRules?.UserIds.Count > 0)
                {
                    var usernames = userlist.Where(p => step.ActRules.UserIds.Contains(p.Id)).ToList();
                    step.ActRules.UserNames = usernames.Select(p => p.UserName).ToList();
                    step.ActRules.UserIds = usernames.Select(p => p.Id).ToList();
                    continue;
                }

                //查找审批角色,采用该申请者所在部门的角色进行查找，找不到往上一级
                if (step.ActRules?.RoleIds != null && step.ActRules?.RoleIds.Count > 0)
                {
                    if (department == null)
                    {
                        throw new Exception(string.Format("流程异常，该申请者没有部门，无法找到{0}的角色为{1}的OA审批人", step.Id, step.ActRules?.RoleNames));
                    }
                    var theroles = rolelist.Where(p => step.ActRules.RoleIds.Contains(p.Id)).ToList();
                    step.ActRules.RoleNames = theroles.Select(p => p.RoleName).ToList();
                    step.ActRules.RoleIds = theroles.Select(p => p.Id).ToList();

                    //待处理部门审批                 
                    var userroles = userrolelist.Where(p => step.ActRules.RoleIds.Contains(p.RoleId)).ToList();

                    bool success = false;
                    var roleDepartment = department;
                    while(roleDepartment != null)
                    {
                        var roleuser = userlist.FirstOrDefault(p => p.DepartmentId == roleDepartment.Id && userroles.Any(q => q.UserId == p.Id));
                        if (roleuser != null)
                        {
                            step.ActRules.UserNames = new List<string> { roleuser.UserName };
                            step.ActRules.UserIds = new List<string> { roleuser.Id };
                            success = true;
                            break;
                        }

                        //指向父级部门进行查找
                        roleDepartment = departmentlist.FirstOrDefault(p => p.Id == roleDepartment.ParentId);
                    }

                    if (success == false)
                    {
                        throw new Exception(string.Format("流程异常，无法找到{0}的角色为{1}的OA审批人", step.Id, step.ActRules?.RoleNames));
                    }
                    else
                    {
                        continue;
                    }
                }

                Type type = Type.GetType(step.StepType, true, true);

                if (type.Name == nameof(OAStartStep))
                {
                    step.ActRules.UserNames = new List<string> { data.CreatorName };
                    step.ActRules.UserIds = new List<string> { data.CreatorId };
                }
                else if (type.IsSubclassOf(typeof(OABaseStep)) && !typeof(IEndStep).IsAssignableFrom(type))
                {
                    throw new Exception(string.Format("流程异常，{0}没有设置OA审批人", step.Id));
                }

                //最后找不到的，如果需要审批那么会指向系统管理员 ToDo
            }

            return oAData;
        }


    }

}
