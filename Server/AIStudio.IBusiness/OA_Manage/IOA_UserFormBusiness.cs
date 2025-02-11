﻿using AIStudio.Entity.DTO.OA_Manage;
using AIStudio.Entity.OA_Manage;
using AIStudio.IBusiness;
using AIStudio.Util.Common;
using AIStudio.Util.DiagramEntity;

namespace AIStudio.Business.OA_Manage
{
    public interface IOA_UserFormBusiness : IBaseBusiness<OA_UserForm>
    {
        Task QueueWork(string id);
        Task<string> DequeueWork(string id);

        void LoadDefinition();
        Task LoadDefinitionAsync();

        Task<PageResult<OA_UserFormDTO>> GetDataListAsync(PageInput<OA_UserFormInputDTO> input);
        new Task<OA_UserFormDTO> GetTheDataAsync(string id);

        Task<List<OA_Step>> PreStepAsync(OA_UserFormDTO data);

        Task SaveDataAsync(OA_UserFormDTO data);
        Task<AjaxResult> EventDataAsync(MyEvent eventData);

        Task DisCardDataAsync(DisCardInput input);

        Task<bool> SuspendAsync(IdInputDTO input);
        Task<bool> ResumeAysnc(IdInputDTO input);
        Task TerminateAsync(IdInputDTO input);
    }

    public class DisCardInput : IdInputDTO
    {
        public string? remark { get; set; }
    }

 
}